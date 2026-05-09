using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv;
using Microsoft.IdentityModel.Tokens;
using Multitool.Api.Exceptions;
using Multitool.Api.Extensions;
using Multitool.Application;
using Multitool.Infrastructure;

namespace Multitool.Api;

public class Program
{
    public static void Main(string[] args)
    {
        RunStartup(args);
    }

    public static void RunStartup(string[] args)
    {
        Env.Load();

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddProblemDetails(configure =>
        {
            configure.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            };
        });

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "");

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontendAndLocalhost", policy =>
            {
                policy.WithOrigins(
                    "https://multitool-frontend-pi.vercel.app/", // Prod Frontend URL
                    "https://multitool-frontend-integration.vercel.app/", // Integration Frontend URL
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
        builder.Services.AddSwaggerGen();
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
                };
            });

        var app = builder.Build();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("AllowFrontendAndLocalhost");

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