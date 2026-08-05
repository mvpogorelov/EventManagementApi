using BookingsService.Application.Abstractions.Persistence.Repositories;
using BookingsService.Application.Abstractions.Services;
using BookingsService.Infrastructure.Persistence;
using BookingsService.Infrastructure.Persistence.Repositories;
using BookingsService.Infrastructure.Services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        
        services.AddScoped<IBookingRepository, BookingRepository>();
        
        services.AddSingleton<IKafkaProducerService>(sp =>
            new KafkaProducerService(
                new ProducerBuilder<string, string>(
                    new ProducerConfig
                    {
                        BootstrapServers = configuration["Kafka:BootstrapServers"],
                        Acks = Acks.All
                    }
                )
                .Build()
            )
        );

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
