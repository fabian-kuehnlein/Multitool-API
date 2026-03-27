using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Multitool.Application.Interfaces;
using Multitool.Application.Services;

namespace Multitool.Application;

public static class Setup
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMapster();

        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<ICustomTableService, CustomTableService>();

        return services;
    }
}
