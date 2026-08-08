using EventManagement.Shared.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventManagement.Shared.Services;

public class OutboxBackgroundService(
    ILogger<OutboxBackgroundService> logger,
    IServiceScopeFactory scopeFactory,
    IKafkaProducerService kafkaProducer)
        : BackgroundService
{
    private const int PollingInterval = 10000;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxBackgroundService запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
                await Task.Delay(PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка при обработке Outbox");
            }
        }

        logger.LogInformation("OutboxBackgroundService остановлен");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var messages = await outboxRepository.GetUnprocessedMessages(ct);

        if (!messages.Any())
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                message.AttemptCount++;

                await kafkaProducer.PublishAsync(message.Topic, message.MessageKey, message.Message, message.MessageType, ct);

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception e)
            {
                logger.LogError(e,
                    "Ошибка отправки Outbox. Topic: {Topic}, MessageKey: {MessageKey}, Message: {Message}, MessageType: {MessageType}",
                    message.Topic, message.MessageKey, message.Message, message.MessageType);

                message.Error = e.Message;
            }
        }

        await outboxRepository.SaveChangesAsync(ct);
    }
}

