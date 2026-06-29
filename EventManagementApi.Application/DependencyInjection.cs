using EventManagement.Application.Abstractions.Services;
using EventManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Application;

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
        services.AddHostedService<BookingBackgroundService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }
}
