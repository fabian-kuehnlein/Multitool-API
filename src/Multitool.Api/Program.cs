using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv;
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
        builder.Services.AddInfrastructure(builder.Configuration);

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

        var app = builder.Build();

        app.UseExceptionHandler();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors("AllowAll");

        app.MapControllers().RequireCors("AllowAll");

        if (app.Environment.IsProduction())
            app.ApplyMigrations();

        app.Run();
    }
}