using System.Reflection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Multitool.Application.Interfaces;
using Multitool.Application.Services;

namespace Multitool.Application;

public static class Setup
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        
        services.AddMapster();

        services.AddScoped<IAuthenticationService, AuthenticationService>();

        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<ICustomTableService, CustomTableService>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}
