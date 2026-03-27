using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.ApiClients;
using Multitool.Infrastructure.Data;
using Multitool.Infrastructure.Repositories;

namespace Multitool.Infrastructure;

public static class Setup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<ICalendarRepository, CalendarRepository>();
        services.AddScoped<ICustomTableRepository, CustomTableRepository>();

        var connectionString = BuildConnectionString(config);

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mySqlOptions => mySqlOptions
                    .EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                ))
        );

        services.AddHttpClient<ICalendarApiClient, CalendarApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://get.api-feiertage.de");
        });

        return services;
    }

    private static string BuildConnectionString(IConfiguration config)
    {
        var dbHost = config["DB_HOST"];
        var dbPort = config["DB_PORT"];
        var dbName = config["DB_NAME"];
        var dbUser = config["DB_USER"];
        var dbPassword = config["DB_PASSWORD"];

        ArgumentException.ThrowIfNullOrEmpty(dbHost);
        ArgumentException.ThrowIfNullOrEmpty(dbPort);
        ArgumentException.ThrowIfNullOrEmpty(dbName);
        ArgumentException.ThrowIfNullOrEmpty(dbUser);
        ArgumentException.ThrowIfNullOrEmpty(dbPassword);

        return $"server={dbHost};port={dbPort};database={dbName};user={dbUser};password={dbPassword}";
    }
}
