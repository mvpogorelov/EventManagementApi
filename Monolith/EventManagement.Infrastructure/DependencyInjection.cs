using EventManagement.Application.Abstractions.Persistence.Repositories;
using EventManagement.Infrastructure.Persistence;
using EventManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using EventManagement.Application.Abstractions.Security;
using EventManagement.Infrastructure.Security;

namespace EventManagement.Infrastructure;

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
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }

    public static IApplicationBuilder ApplyMigrations(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.Migrate();
        }

        return app;
    }
}
