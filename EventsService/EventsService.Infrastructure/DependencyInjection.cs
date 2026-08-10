using EventManagement.Shared.Abstractions;
using EventManagement.Shared.Models;
using EventManagement.Shared.Services;
using EventsService.Application.Abstractions.Persistence.Repositories;
using EventsService.Infrastructure.Persistence;
using EventsService.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

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
        services.AddScoped<IInboxRepository, InboxRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<ICachedEventRepository, CachedEventRepository>();

        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));

        var kafkaSettings = configuration.GetSection("Kafka").Get<KafkaSettings>() ?? throw new InvalidOperationException(nameof(KafkaSettings));

        services.AddSingleton<IKafkaProducerService>(sp => new KafkaProducerService(kafkaSettings));
        services.AddHostedService<KafkaConsumerBackgroundService>();
        services.AddHostedService<OutboxBackgroundService>();

        var redisConnectionString = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis Connection String");

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(
                new ConfigurationOptions
                {
                    EndPoints = { redisConnectionString },
                    ConnectTimeout = 5000,
                    SyncTimeout = 3000,
                    AbortOnConnectFail = false,
                    ConnectRetry = 3,
                })
        );

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
