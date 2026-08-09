using Confluent.Kafka;
using EventManagement.Shared.Abstractions;
using EventManagement.Shared.Entities;
using EventManagement.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace EventManagement.Shared.Services;

public class KafkaConsumerBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaSettings> kafkaSettings,
    ILogger<KafkaConsumerBackgroundService> logger)
        : BackgroundService
{
    private const int MaxConsumeExeptions = 10;
    private const int MaxCriticalExeptions = 10;

    protected async override Task ExecuteAsync(CancellationToken stoppingToken) =>
        await Task.Run(async () =>
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = kafkaSettings.Value.BootstrapServers,
                GroupId = kafkaSettings.Value.ConsumerGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoOffsetStore = false,
                EnableAutoCommit = false
            };
            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(kafkaSettings.Value.ComsumerTopic);
            logger.LogInformation("Kafka Consumer запущен. Топик: {ComsumerTopic}", kafkaSettings.Value.ComsumerTopic);

            try
            {
                int errorCount = 0;

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);

                        if (consumeResult == null) continue;

                        await ProcessMessageAsync(consumeResult, stoppingToken);

                        consumer.Commit(consumeResult);
                        errorCount = 0;
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogInformation("Kafka Consumer остановлен. Топик: {ComsumerTopic}", kafkaSettings.Value.ComsumerTopic);

                        break;
                    }
                    catch (ConsumeException e)
                    {
                        errorCount++;
                        logger.LogError(e, $"Kafka ConsumeException");

                        if (errorCount > MaxConsumeExeptions)
                        {
                            throw;
                        }

                        var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, errorCount), 60));

                        await Task.Delay(delay, stoppingToken);
                    }

                    catch (Exception e)
                    {
                        errorCount++;
                        logger.LogCritical(e, $"Kafka критическая ошибка");

                        if (errorCount > MaxCriticalExeptions)
                        {
                            throw;
                        }

                        var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, errorCount), 60));

                        await Task.Delay(delay, stoppingToken);
                    }
                }
            }
            finally
            {
                consumer.Close();
            }
        },
        stoppingToken);

    private async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var inboxRepository = scope.ServiceProvider.GetRequiredService<IInboxRepository>();

        var inbox = new Inbox
        {
            Topic = kafkaSettings.Value.ComsumerTopic,
            MessageKey = consumeResult.Message.Key,
            MessageType = GetHeaderValue(consumeResult.Message.Headers, "message-type"),
            Message = consumeResult.Message.Value
        };

        await inboxRepository.CreateAsync(inbox, ct);
    }

    private string? GetHeaderValue(Headers headers, string key)
    {
        if (headers != null && headers.TryGetLastBytes(key, out var headerBytes))
        {
            return Encoding.UTF8.GetString(headerBytes);
        }

        return null;
    }
}
