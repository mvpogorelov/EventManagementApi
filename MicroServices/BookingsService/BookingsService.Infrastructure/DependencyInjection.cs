using BookingsService.Application.Abstractions.Persistence.Repositories;
using BookingsService.Infrastructure.Persistence;
using BookingsService.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingsService.Infrastructure;

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
