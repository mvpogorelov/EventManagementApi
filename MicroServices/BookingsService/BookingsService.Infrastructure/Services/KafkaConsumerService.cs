using BookingsService.Application.Abstractions.Persistence.Repositories;
using BookingsService.Application.Services;
using Confluent.Kafka;
using EventManagement.Contracts.Kafka;
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
    IOptions<KafkaTopics> kafkaTopics,
    ILogger<KafkaConsumerService> logger)
        : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await Task.Run(async () =>
        {
            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(kafkaTopics.Value.Events);
            logger.LogInformation($"Kafka Consumer подписка оформлена на '{kafkaTopics.Value.Events}'");

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
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

            var message = consumeResult.Message.Value;
            var messageType = GetHeaderValue(consumeResult.Message.Headers, "message-type");

            switch (messageType)
            {
                case nameof(EventAllowed):
                    var eventAllowed = JsonSerializer.Deserialize<EventAllowed>(message);
                    var allowedBooking = await bookingRepository.GetByIdAsync(eventAllowed.BookingId, ct);

                    if (allowedBooking is not null)
                    {
                        allowedBooking.Confirm();
                        await bookingRepository.UpdateAsync(allowedBooking, ct);
                    }
                    break;

                case nameof(EventDisabled):
                    var eventDisabled = JsonSerializer.Deserialize<EventDisabled>(message);
                    var disabledBooking = await bookingRepository.GetByIdAsync(eventDisabled.BookingId, ct);

                    if (disabledBooking is not null)
                    {
                        disabledBooking.Reject(eventDisabled.Reason);
                        await bookingRepository.UpdateAsync(disabledBooking, ct);
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
