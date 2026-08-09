using EventsService.Application.Abstractions.Services;
using EventsService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventsService.Application;

/// <summary>
/// Extensions для добавления в DI бизнес логики
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Extension для добавления в DI бизнес логики
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddHostedService<EventsInboxBackgroundService>();
        services.AddScoped<IEventService, EventService>();

        return services;
    }
}
