using EventManagmentApi.Application.Enums;
using EventManagmentApi.Application.Exceptions;
using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Application.Models;
using System.Collections.Concurrent;

namespace EventManagmentApi.Application.Services;

/// <summary>
/// Сервис для работы с бронью
/// </summary>
public class BookingService(IEventService eventService) : IBookingService
{
    private static ConcurrentDictionary<Guid, Booking> _booking = new();

    /// <summary>
    /// Получение списка брони
    /// </summary>
    /// <param name="status">Фильтр по статусу</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Список брони</returns>
    public async Task<IReadOnlyList<Booking>> GetByStatusAsync(BookingStatusEnum status, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var booking = _booking.Values as IEnumerable<Booking>;

        return booking
            .Where(b => b.Status == status)
            .ToList();
    }

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    public async Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_booking.TryGetValue(bookingId, out var booking))
        {
            return booking;
        }

        throw new NotFoundException($"Бронь с Id: {bookingId} не найдена");
    }

    /// <summary>
    /// Создание брони
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var @event = eventService.Get(eventId);

        if (@event is not null)
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                EventId = @event.Id,
                Status = BookingStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };

            if (_booking.TryAdd(booking.Id, booking))
            {
                return booking;
            }
        }

        throw new ArgumentException($"Не удалось создать бронь для события: {eventId}");
    }

    /// <summary>
    /// Обновление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="status">Статус брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <exception cref="NotFoundException">Если бронь не найдена</exception>
    public async Task UpdateStatusAsync(Guid id, BookingStatusEnum status, CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            if (!_booking.TryGetValue(id, out var booking))
            {
                throw new NotFoundException($"Бронь с Id: {id} не найдена");
            }

            var updatedBooking = booking with { Status = status, ProcessedAt = DateTime.UtcNow };

            if (_booking.TryUpdate(id, updatedBooking, booking))
            {
                break;
            }
        }
    }

    /// <summary>
    /// Удаление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <exception cref="NotFoundException">Если бронь не найдена</exception>
    public void Remove(Guid id)
    {
        if (!_booking.TryGetValue(id, out var booking))
        {
            throw new NotFoundException($"Бронь с Id: {id} не найдена");
        }

        _booking.TryRemove(new KeyValuePair<Guid, Booking>(id, booking));
    }
}
