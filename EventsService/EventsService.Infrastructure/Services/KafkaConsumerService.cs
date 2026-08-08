using Confluent.Kafka;
using EventManagement.Contracts.Kafka;
using EventManagement.Shared.Models;
using EventsService.Application.Abstractions.Persistence.Repositories;
using EventsService.Application.Abstractions.Services;
using EventsService.Application.Services;
using EventsService.Domain.Common;
using EventsService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace EventsService.Infrastructure.Services;

public class KafkaConsumerService(
    ConsumerConfig config,
    IServiceProvider serviceProvider,
    IOptions<KafkaTopics> kafkaTopics,
    ILogger<KafkaConsumerService> logger)
        : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await Task.Run(async () =>
        {
            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(kafkaTopics.Value.Bookings);
            logger.LogInformation($"Kafka Consumer подписка оформлена на '{kafkaTopics.Value.Bookings}'");

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
        var inboxRepository = scope.ServiceProvider.GetRequiredService<IInboxRepository>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        Inbox? inbox = null;

        try
        {
            var topic = kafkaTopics.Value.Bookings;
            var messageKey = consumeResult.Message.Key;
            var messageType = GetHeaderValue(consumeResult.Message.Headers, "message-type");
            var message = consumeResult.Message.Value;

            inbox = await inboxRepository.GetByMessageAsync(topic, messageKey, messageType, ct);

            if (inbox is not null)
            {
                return;
            }

            inbox = new Inbox(topic, messageKey, message, DateTime.UtcNow, InboxStatus.Processing, messageType);
            await inboxRepository.CreateAsync(inbox);

            switch (messageType)
            {
                case nameof(BookingConfirmed):
                    var bookingConfirmed = JsonSerializer.Deserialize<BookingConfirmed>(message);

                    await eventService.ApproveBookingAsync(
                        bookingConfirmed.EventId,
                        bookingConfirmed.UserId,
                        bookingConfirmed.BookingId,
                        bookingConfirmed.Seats,
                        ct);

                    inbox.EventId = bookingConfirmed.EventId;
                    inbox.UserId = bookingConfirmed.UserId;
                    inbox.BookingId = bookingConfirmed.BookingId;
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
