using BookingsService.Application.Abstractions.Services;
using BookingsService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookingsService.Application;

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
        services.AddHostedService<BookingsInboxBackgroundService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }
}
