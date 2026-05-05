using EventManagmentApi.Application.Enums;
using EventManagmentApi.Application.Exceptions;
using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Application.Models;
using System.Collections.Concurrent;

namespace EventManagmentApi.Application.Services;

/// <summary>
/// Сервис для работы с бронью
/// </summary>
public class BookingService : IBookingService
{
    private static ConcurrentDictionary<Guid, Booking> _booking = new();

    /// <summary>
    /// Получение списка брони
    /// </summary>
    /// <param name="eventId">Фильтр по событию</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <returns>Список событий</returns>
    public PaginatedResult<Booking> GetAll(Guid? eventId, int page = 1, int pageSize = 10)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException($"Неверный номер страницы: {nameof(page)}");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException($"Неверный размер страницы: {nameof(pageSize)}");
        }

        var booking = _booking.Values as IEnumerable<Booking>;

        if (eventId.HasValue)
        {
            booking = booking.Where(b => b.EventId == eventId.Value);
        }

        var totalItems = booking.Count();
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        var items = booking
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<Booking>(items, page, items.Count, totalItems, totalPages);
    }

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <returns>Бронь</returns>
    public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
    {
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
    /// <returns>Бронь</returns>
    public async Task<Booking> CreateBookingAsync(Guid eventId)
    {
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatusEnum.Pending,
            CreatedAt = DateTime.UtcNow
        };

        if (_booking.TryAdd(booking.Id, booking))
        {
            return booking;
        }

        throw new ArgumentException("Не удалось создать бронь");
    }

    /// <summary>
    /// Обновление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="status">Статус брони</param>
    /// <exception cref="NotFoundException">Если бронь не найдена</exception>
    public async Task UpdateStatusAsync(Guid id, BookingStatusEnum status)
    {
        while (true)
        {
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
