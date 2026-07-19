using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Multitool.Domain.Interfaces;
using Multitool.Infrastructure.ApiClients;
using Multitool.Infrastructure.Authentification;
using Multitool.Infrastructure.Data;
using Multitool.Infrastructure.Repositories;

namespace Multitool.Infrastructure;

public static class Setup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string is missing. Set DB_CONNECTION_STRING in environment variables.");

        services.AddScoped<IAdminKeyProvider, AdminKeyProvider>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<ICalendarRepository, CalendarRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICustomTableRepository, CustomTableRepository>();
        services.AddScoped<ITodoRepository, TodoRepository>();

        services.AddScoped<IWorkDayRepository, WorkDayRepository>();
        services.AddScoped<IWeekSummaryRepository, WeekSummaryRepository>();
        services.AddScoped<IWorkTimeSettingsRepository, WorkTimeSettingsRepository>();

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
