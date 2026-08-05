using Confluent.Kafka;
using EventsService.Application.Abstractions.Persistence.Repositories;
using EventsService.Application.Abstractions.Services;
using EventsService.Infrastructure.Persistence;
using EventsService.Infrastructure.Persistence.Repositories;
using EventsService.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IEventRepository, EventRepository>();
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
