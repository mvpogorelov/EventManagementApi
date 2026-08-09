using EventManagement.Application.Abstractions.Persistence.Repositories;
using EventManagement.Application.Abstractions.Services;
using EventManagement.Domain.Common;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using System.Data;

namespace EventManagement.Application.Services;

/// <summary>
/// Сервис для работы с бронью
/// </summary>
public class BookingService(
    IBookingRepository bookingRepository,
    IEventRepository eventRepository,
    IUserRepository userRepository)
        : IBookingService
{
    private const int UserBookingLimit = 10;
    private static readonly SemaphoreSlim _createSemaphore = new(1, 1);

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
    public async Task<Booking> CreateBookingAsync(Guid eventId, Guid userId, CancellationToken ct = default)
    {
        await _createSemaphore.WaitAsync(ct);

        try
        {
            var @event = await eventRepository.GetByIdAsync(eventId, ct)
                ?? throw new NotFoundException($"Событие не найдено: {eventId}");
            var user = await userRepository.GetByIdAsync(userId, ct)
                ?? throw new NotFoundException($"Пользователь не найден: {userId}");

            if (@event.StartAt < DateTime.UtcNow)
            {
                throw new PastEventBookingException("Событие уже началось");
            }

            if (user.Bookings
                .Count(b =>
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)
                    && b.Event?.StartAt >= DateTime.UtcNow)
                >= UserBookingLimit)
            {
                throw new BookingLimitException($"Превышен лимит активных броней: {UserBookingLimit}");
            }

            if (!@event.TryReserveSeats())
            {
                throw new NoAvailableSeatsException($"Нет доступных мест для события: {eventId}");
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            return await bookingRepository.CreateAsync(booking, ct);
        }
        finally
        {
            _createSemaphore.Release();
        }
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
