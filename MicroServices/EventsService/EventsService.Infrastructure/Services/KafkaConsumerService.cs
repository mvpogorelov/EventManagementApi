using Confluent.Kafka;
using EventManagement.Contracts.Kafka;
using EventsService.Application.Abstractions.Persistence.Repositories;
using EventsService.Application.Abstractions.Services;
using EventsService.Application.Services;
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
        try
        {
            using var scope = serviceProvider.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

            var message = consumeResult.Message.Value;
            var messageType = GetHeaderValue(consumeResult.Message.Headers, "message-type");

            switch(messageType)
            {
                case nameof(BookingProcessing):
                    var bookingProcessing = JsonSerializer.Deserialize<BookingProcessing>(message);

                    await eventService.CheckBookingAsync(
                        bookingProcessing.EventId,
                        bookingProcessing.UserId,
                        bookingProcessing.BookingId,
                        bookingProcessing.Seats,
                        ct);
                    break;

                case nameof(BookingCancelled):
                    var bookingCancelled = JsonSerializer.Deserialize<BookingCancelled>(message);
                    var cancelledBooking = await bookingRepository.GetByIdAsync(bookingCancelled.BookingId, ct);

                    if (cancelledBooking is not null)
                    {
                        await bookingRepository.DeleteAsync(cancelledBooking, ct);
                    }
                    break;
                
                case nameof(BookingRemoved):
                    var bookingRemoved = JsonSerializer.Deserialize<BookingRemoved>(message);
                    var removedBooking = await bookingRepository.GetByIdAsync(bookingRemoved.BookingId, ct);

                    if (removedBooking is not null)
                    {
                        await bookingRepository.DeleteAsync(removedBooking, ct);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Ошибка при обработке сообщения с ключом {consumeResult.Message.Key}");
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
