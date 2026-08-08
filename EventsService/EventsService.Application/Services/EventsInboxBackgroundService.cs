using EventManagement.Contracts.Kafka;
using EventManagement.Shared.Abstractions;
using EventManagement.Shared.Entities;
using EventManagement.Shared.Models;
using EventManagement.Shared.Services;
using EventsService.Application.Abstractions.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EventsService.Application.Services;

public class EventsInboxBackgroundService : InboxBackgroundService
{
    private readonly KafkaSettings _kafkaSettings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IKafkaProducerService _kafkaProducer;
    
    public EventsInboxBackgroundService(
        ILogger<EventsInboxBackgroundService> logger,
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
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var inbox = await eventRepository.GetInboxByIdAsync(inboxId, ct);

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
            case nameof(BookingCanceled):
                var bookingCanceled = JsonSerializer.Deserialize<BookingCanceled>(inbox.Message);
                var existingCanceledInbox = await eventRepository.GetInboxByMessageTypeAndBookingId(Topic, inbox.MessageType, bookingCanceled.BookingId, ct);

                bookingId = bookingCanceled.BookingId;
                eventId = bookingCanceled.EventId;
                userId = bookingCanceled.UserId;

                if (existingCanceledInbox is not null)
                {
                    error = "Дубль";
                }
                else
                {
                    var @event = await eventRepository.GetByIdAsync(bookingCanceled.EventId, ct);

                    if (@event is null)
                    {
                        error = "Событие не найдено";
                    }
                    else
                    {
                        error = null;
                        @event.ReleaseSeats(bookingCanceled.Seats);
                    }
                }

                break;
            
            case nameof(BookingRemoved):
                var bookingRemoved = JsonSerializer.Deserialize<BookingRemoved>(inbox.Message);
                var existingRemovedInbox = await eventRepository.GetInboxByMessageTypeAndBookingId(Topic, inbox.MessageType, bookingRemoved.BookingId, ct);

                bookingId = bookingRemoved.BookingId;
                eventId = bookingRemoved.EventId;
                userId = bookingRemoved.UserId;

                if (existingRemovedInbox is not null)
                {
                    error = "Дубль";
                }
                else
                {
                    var @event = await eventRepository.GetByIdAsync(bookingRemoved.EventId, ct);

                    if (@event is null)
                    {
                        error = "Событие не найдено";
                    }
                    else
                    {
                        error = null;
                        @event.ReleaseSeats(bookingRemoved.Seats);
                    }
                }

                break;
            
            case nameof(BookingCreated):
                var bookingCreated = JsonSerializer.Deserialize<BookingCreated>(inbox.Message);
                var existingCreatedInbox = await eventRepository.GetInboxByMessageTypeAndBookingId(Topic, inbox.MessageType, bookingCreated.BookingId, ct);

                bookingId = bookingCreated.BookingId;
                eventId = bookingCreated.EventId;
                userId = bookingCreated.UserId;

                if (existingCreatedInbox is not null)
                {
                    error = "Дубль";
                }
                else
                {
                    var @event = await eventRepository.GetByIdAsync(bookingCreated.EventId, ct);

                    if (@event is null)
                    {
                        error = "Событие не найдено";
                    }
                    else if (@event.StartAt < DateTime.UtcNow)
                    {
                        error = "Событие уже началось";
                    }
                    else if (!@event.TryReserveSeats(bookingCreated.Seats))
                    {
                        error = "Нет доступных мест";
                    }
                    else
                    {
                        error = null;
                    }

                    if (error is null)
                    {
                        var kafkaMessage = new BookingConfirmed
                        {
                            BookingId = bookingId.Value,
                            EventId = eventId.Value,
                            UserId = userId.Value,
                            ConfirmedAt = DateTime.UtcNow
                        };
                        var outbox = new Outbox
                        {
                            Topic = _kafkaSettings.Topics.Events,
                            MessageKey = eventId.Value.ToString(),
                            MessageType = _kafkaProducer.GetMessageType(kafkaMessage),
                            Message = _kafkaProducer.GetMessageString(kafkaMessage)
                        };

                        await eventRepository.AddOutboxAsync(outbox, ct);
                    }
                    else
                    {
                        var kafkaMessage = new BookingRejected
                        {
                            BookingId = bookingId.Value,
                            EventId = eventId.Value,
                            UserId = userId.Value,
                            RejectedAt = DateTime.UtcNow,
                            Reason = error
                        };
                        var outbox = new Outbox
                        {
                            Topic = _kafkaSettings.Topics.Events,
                            MessageKey = eventId.Value.ToString(),
                            MessageType = _kafkaProducer.GetMessageType(kafkaMessage),
                            Message = _kafkaProducer.GetMessageString(kafkaMessage)
                        };

                        await eventRepository.AddOutboxAsync(outbox, ct);
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

        await eventRepository.SaveChangesAsync(ct);
    }
}