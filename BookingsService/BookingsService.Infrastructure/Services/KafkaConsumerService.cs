using BookingsService.Application.Abstractions.Persistence.Repositories;
using BookingsService.Application.Services;
using BookingsService.Domain.Common;
using BookingsService.Domain.Entities;
using Confluent.Kafka;
using EventManagement.Contracts.Kafka;
using EventManagement.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace BookingsService.Infrastructure.Services;

public class KafkaConsumerService(
    ConsumerConfig config,
    IServiceProvider serviceProvider,
    IOptions<KafkaSettings> kafkaSettings,
    ILogger<KafkaConsumerService> logger)
        : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await Task.Run(async () =>
        {
            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(kafkaSettings.Value.Topics.Events);
            logger.LogInformation($"Kafka Consumer подписка оформлена на '{kafkaSettings.Value.Topics.Events}'");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);

                        if (consumeResult == null) continue;

                        await ProcessMessageAsync(consumeResult, stoppingToken);

                        consumer.Commit(consumeResult);
                    }
                    catch (ConsumeException e)
                    {
                        logger.LogError($"Ошибка чтения из Kafka: {e.Error.Reason}");
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Критическая ошибка в цикле консьюмера");
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
        using var scope = serviceProvider.CreateScope();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var inboxRepository = scope.ServiceProvider.GetRequiredService<IInboxRepository>();
        Inbox? inbox = null;

        try
        {
            var topic = kafkaSettings.Value.Topics.Events;
            var messageKey = consumeResult.Message.Key;
            var message = consumeResult.Message.Value;
            var messageType = GetHeaderValue(consumeResult.Message.Headers, "message-type");

            inbox = await inboxRepository.GetByMessageAsync(topic, messageKey, messageType, ct);

            if (inbox is not null)
            {
                return;
            }

            inbox = new Inbox(topic, messageKey, message, DateTime.UtcNow, InboxStatus.Processing, messageType);
            await inboxRepository.CreateAsync(inbox);

            switch (messageType)
            {
                case nameof(BookingRejected):
                    var bookingRejected = JsonSerializer.Deserialize<BookingRejected>(message);
                    var rejectedBooking = await bookingRepository.GetByIdAsync(bookingRejected.BookingId, ct);

                    if (rejectedBooking is not null)
                    {
                        rejectedBooking.Reject(bookingRejected.Reason);
                        await bookingRepository.UpdateAsync(rejectedBooking, ct);
                    }

                    inbox.Status = InboxStatus.Processed;
                    await inboxRepository.UpdateAsync(inbox);

                    break;

                default:
                    inbox.Status = InboxStatus.Failed;
                    inbox.StatusComment = "Нет обработчика";

                    await inboxRepository.UpdateAsync(inbox);

                    break;
            }
        }
        catch (Exception ex)
        {
            var errorMessage = ex.InnerException?.Message ?? ex.Message;

            if (inbox is not null)
            {
                inbox.Status = InboxStatus.Failed;
                inbox.StatusComment = $"{errorMessage}";

                await inboxRepository.UpdateAsync(inbox);
            }

            logger.LogError(ex, "При обработке сообщения возникла ошибка: {Error}", errorMessage);
        }
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
