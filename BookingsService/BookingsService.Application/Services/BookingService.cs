using BookingsService.Application.Abstractions.Persistence.Repositories;
using BookingsService.Application.Abstractions.Services;
using BookingsService.Domain.Common;
using BookingsService.Domain.Entities;
using BookingsService.Domain.Exceptions;
using EventManagement.Contracts.Common;
using EventManagement.Contracts.Kafka;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace BookingsService.Application.Services;

/// <summary>
/// Сервис для работы с бронью
/// </summary>
public class BookingService(
    IBookingRepository bookingRepository,
    IKafkaProducerService kafkaProducer,
    IOptions<KafkaTopics> kafkaTopics)
        : IBookingService
{
    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="role">Роль пользователя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    public async Task<Booking> GetBookingByIdAsync(Guid bookingId, Guid userId, UserRole userRole, CancellationToken ct = default)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId, ct)
            ?? throw new NotFoundException($"Бронь с Id: {bookingId} не найдена");

        CheckOperationAllowed(booking.UserId, userId, userRole);

        return booking;
    }

    /// <summary>
    /// Создание брони
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    public async Task<Booking> CreateBookingAsync(Guid eventId, int seats, Guid userId, CancellationToken ct = default)
    {
        if (seats <= 0)
        {
            throw new ValidationException("Мест должно быть больше 0");
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Seats = seats,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        await bookingRepository.CreateAsync(booking, ct);
        await kafkaProducer.PublishAsync(kafkaTopics.Value.Bookings,
            booking.EventId.ToString(),
            new BookingConfirmed
            {
                BookingId = booking.Id,
                EventId = booking.EventId,
                UserId = booking.UserId,
                Seats = booking.Seats,
                ConfirmedAt = DateTime.UtcNow
            });

        return booking;
    }

    /// <summary>
    /// Удаление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <exception cref="NotFoundException">Если бронь не найдена</exception>
    public async Task RemoveAsync(Guid id, Guid userId, UserRole userRole, CancellationToken ct = default)
    {
        var booking = await bookingRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Бронь с Id: {id} не найдена");

        CheckOperationAllowed(booking.UserId, userId, userRole);

        await bookingRepository.DeleteAsync(booking);
    }

    /// <summary>
    /// Отмена брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="currentUserId">Идентификатор пользователя</param>
    /// <param name="ct">Токен отмены</param>
    public async Task CancelAsync(Guid id, Guid userId, UserRole userRole, CancellationToken ct = default)
    {
        var booking = await bookingRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Бронь с Id: {id} не найдена");

        CheckOperationAllowed(booking.UserId, userId, userRole);

        booking.Cancel();
        await bookingRepository.UpdateAsync(booking, ct);
    }

    private void CheckOperationAllowed(Guid bookingUserId, Guid userId, UserRole userRole)
    {
        if (userRole != UserRole.Admin && bookingUserId != userId)
        {
            throw new OperationNotAllowedException();
        }
    }
}

