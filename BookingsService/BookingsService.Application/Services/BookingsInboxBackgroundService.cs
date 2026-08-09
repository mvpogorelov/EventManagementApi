using BookingsService.Application.Abstractions.Persistence.Repositories;
using EventManagement.Contracts.Kafka;
using EventManagement.Shared.Abstractions;
using EventManagement.Shared.Models;
using EventManagement.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BookingsService.Application.Services;

public class BookingsInboxBackgroundService : InboxBackgroundService
{
    private readonly KafkaSettings _kafkaSettings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IKafkaProducerService _kafkaProducer;

    public BookingsInboxBackgroundService(
        ILogger<BookingsInboxBackgroundService> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaSettings> kafkaSettings,
        IKafkaProducerService kafkaProducer)
        : base(logger, scopeFactory)
    {
        _kafkaSettings = kafkaSettings.Value;
        _scopeFactory = scopeFactory;
        _kafkaProducer = kafkaProducer;
    }

    protected override string Topic => _kafkaSettings.ComsumerTopic;

    protected override async Task ProcessBusinessLogicAsync(int inboxId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var inbox = await bookingRepository.GetInboxByIdAsync(inboxId, ct);

        if (inbox == null)
        {
            return;
        }

        inbox.AttemptCount++;

        string? error = null;
        Guid? bookingId = null;
        Guid? eventId = null;
        Guid? userId = null;

        switch (inbox.MessageType)
        {
            case nameof(BookingRejected):
                var bookingRejected = JsonSerializer.Deserialize<BookingRejected>(inbox.Message);
                var existingRejectedInbox = await bookingRepository.GetInboxByMessageTypeAndBookingId(Topic, inbox.MessageType, bookingRejected.BookingId, ct);

                bookingId = bookingRejected.BookingId;
                eventId = bookingRejected.EventId;
                userId = bookingRejected.UserId;

                if (existingRejectedInbox is not null)
                {
                    error = "Дубль";
                }
                else
                {
                    var booking = await bookingRepository.GetByIdAsync(bookingRejected.BookingId, ct);

                    if (booking is null)
                    {
                        error = "Бронь не найдена";
                    }
                    else
                    {
                        error = null;
                        booking.Reject(bookingRejected.Reason);
                    }
                }

                break;

            case nameof(BookingConfirmed):
                var bookingConfirmed = JsonSerializer.Deserialize<BookingConfirmed>(inbox.Message);
                var existingConfirmedInbox = await bookingRepository.GetInboxByMessageTypeAndBookingId(Topic, inbox.MessageType, bookingConfirmed.BookingId, ct);

                bookingId = bookingConfirmed.BookingId;
                eventId = bookingConfirmed.EventId;
                userId = bookingConfirmed.UserId;

                if (existingConfirmedInbox is not null)
                {
                    error = "Дубль";
                }
                else
                {
                    var booking = await bookingRepository.GetByIdAsync(bookingConfirmed.BookingId, ct);

                    if (booking is null)
                    {
                        error = "Бронь не найдена";
                    }
                    else
                    {
                        error = null;
                        booking.Confirm();
                    }
                }

                break;

            default:
                error = "Отсутствует обработчик";

                break;
        }

        inbox.ProcessedAt = DateTime.UtcNow;
        inbox.Error = error;
        inbox.BookingId = bookingId;
        inbox.EventId = eventId;
        inbox.UserId = userId;

        await bookingRepository.SaveChangesAsync(ct);
    }
}
