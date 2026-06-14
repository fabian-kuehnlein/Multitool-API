using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;
using Multitool.Api.Exceptions;
using Multitool.Api.Extensions;
using Multitool.Application;
using Multitool.Domain.Exceptions;
using Multitool.Infrastructure;
using Multitool.Api.BackgroundJobs;
using Multitool.Api.Configuration;

namespace Multitool.Api;

public class Program
{
    public static void Main(string[] args)
    {
        RunStartup(args);
    }

    public static void RunStartup(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddProblemDetails(configure =>
        {
            configure.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            };
        });

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(connectionString!);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontendAndLocalhost", policy =>
            {
                policy.WithOrigins(
                    "https://multitool-frontend.netlify.app/", // Prod Frontend URL
                    "https://multitool-frontend-integration.netlify.app", // Integration Frontend URL
                    "http://localhost:4200" // Angular local development on ng serve
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            });
        });

        builder.Services.AddControllers()
            .AddJsonOptions(opts =>
            opts.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddHostedService<CleanupPastEventsService>();
        builder.Services.Configure<CronJobSettings>(builder.Configuration.GetSection("CronJobs"));

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Gib hier deinen JWT ein"
            });

            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

            c.IncludeXmlComments(xmlPath);
        });

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var jwtKey = builder.Configuration["Jwt:Key"]
            ?? throw new JwtMissingException("JWT key is missing.");

        builder.Services
            .AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("login-limit", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        var app = builder.Build();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("AllowFrontendAndLocalhost");

        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers().RequireCors("AllowFrontendAndLocalhost");

        // Apply migrations on startup in production, or in development if enabled via environment variable
        if (app.Environment.IsProduction() || (app.Environment.IsDevelopment() && Environment.GetEnvironmentVariable("APPLY_MIGRATIONS") == "true"))
        {
            app.ApplyMigrations();
        }

        app.Run();
    }
}