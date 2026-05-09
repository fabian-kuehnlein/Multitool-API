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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string is missing. Set DB_CONNECTION_STRING in environment variables.");

        services.AddScoped<ICalendarRepository, CalendarRepository>();
        services.AddScoped<ICustomTableRepository, CustomTableRepository>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null
                    );
                })
        );

        services.AddHttpClient<ICalendarApiClient, CalendarApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://get.api-feiertage.de");
        });

        return services;
    }
}
