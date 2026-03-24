using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Multitool.Application.Interfaces;
using Multitool.Application.Mappings;
using Multitool.Application.Services;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.Data;
using Multitool.Infrastructure.Repositories;

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

        // Calendar Infrastructure
        builder.Services.AddScoped<ICalendarService, CalendarService>();
        builder.Services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();

        // Custom-Table Infrastructure
        builder.Services.AddScoped<ICustomTableRepository, CustomTableRepository>();
        builder.Services.AddScoped<ICustomTableService, CustomTableService>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
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
        builder.Services.AddAutoMapper(typeof(MappingProfile));

        builder.Services.AddHttpClient<ICalendarEventRepository, CalendarEventRepository>(client =>
        {
            client.BaseAddress = new Uri("https://get.api-feiertage.de");
        });

        var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

        Console.WriteLine($"DB Connection: {dbUser}@{dbHost}:{dbPort}/{dbName}");

        var connectionString = $"server={dbHost};port={dbPort};database={dbName};user={dbUser};password={dbPassword}";

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(11, 7, 2)),
                mySqlOptions => mySqlOptions
                    .EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                ))
            );

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();
        app.UseCors("AllowAll");

        app.MapControllers().RequireCors("AllowAll");

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            if (builder.Environment.IsProduction())
            {
                dbContext.Database.Migrate();
            }
        };

        app.Run();
    }
}