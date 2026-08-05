using EventsService.Application.Abstractions.Persistence.Repositories;
using EventsService.Infrastructure.Persistence;
using EventsService.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventsService.Infrastructure;

/// <summary>
/// Extensions для добавления в DI инфраструктуры
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Extension для добавления в DI инфраструктуры
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        return services;
    }

    public static IApplicationBuilder ApplayMigrations(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.Migrate();
        }

        return app;
    }
}
