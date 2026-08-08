using BookingsService.Application.Abstractions.Persistence.Repositories;
using BookingsService.Application.Abstractions.Services;
using BookingsService.Infrastructure.Persistence;
using BookingsService.Infrastructure.Persistence.Repositories;
using BookingsService.Infrastructure.Services;
using Confluent.Kafka;
using EventManagement.Shared.Abstractions;
using EventManagement.Shared.Services;
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
        services.AddScoped<IInboxRepository, InboxRepository>();

        var bootstrapServers = configuration["Kafka:BootstrapServers"];

        services.AddSingleton<IKafkaProducerService>(sp =>
            new KafkaProducerService(
                new ProducerBuilder<string, string>(
                    new ProducerConfig
                    {
                        BootstrapServers = bootstrapServers,
                        Acks = Acks.All
                    }
                )
                .Build()
            )
        );

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = "event-processing-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoOffsetStore = false,
            EnableAutoCommit = false
        };
        services.AddSingleton(consumerConfig);

        services.AddHostedService<KafkaConsumerService>();

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
