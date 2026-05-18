using EventManagmentApi.Application.Enums;
using EventManagmentApi.Application.Exceptions;
using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Application.Models;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace EventManagmentApi.Application.Services;

/// <summary>
/// Сервис для работы с бронью
/// </summary>
public class BookingService(IEventService eventService, ILogger<BookingService> logger) : IBookingService
{
    private readonly object _bookingLock = new();
    private static ConcurrentDictionary<Guid, Booking> _booking = new();
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    /// <summary>
    /// Получение списка брони
    /// </summary>
    /// <param name="status">Фильтр по статусу</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Список брони</returns>
    public Task<ReadOnlyCollection<Booking>> GetByStatusAsync(BookingStatus status, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var booking = _booking.Values as IEnumerable<Booking>;

        return Task.FromResult(booking
            .Where(b => b.Status == status)
            .ToList()
            .AsReadOnly());
    }

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    public Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_booking.TryGetValue(bookingId, out var booking))
        {
            return Task.FromResult(booking);
        }

        throw new NotFoundException($"Бронь с Id: {bookingId} не найдена");
    }

    /// <summary>
    /// Создание брони
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    public Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (_bookingLock)
        {
            eventService.TryReserveSeats(eventId);
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _booking.AddOrUpdate(booking.Id, booking, (key, existing) => existing);

        return Task.FromResult(booking);
    }

    /// <summary>
    /// Обновление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="status">Статус брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <exception cref="NotFoundException">Если бронь не найдена</exception>
    public Task UpdateStatusAsync(Guid id, BookingStatus status, CancellationToken ct)
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

        return Task.CompletedTask;
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


    /// <summary>
    /// Обработка брони
    /// </summary>
    /// <param name="booking">Бронь</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task ProcessBookingAsync(Booking booking, CancellationToken ct)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), ct);

        await _processingSemaphore.WaitAsync(ct);
        try
        {
            var @event = eventService.Get(booking.EventId);

            if (@event is null)
            {
                await UpdateStatusAsync(booking.Id, BookingStatus.Rejected, ct);

                logger.LogWarning($"Бронь {booking.Id} отклонена, отсутствует событие {booking.EventId}");
            }
            else
            {
                await UpdateStatusAsync(booking.Id, BookingStatus.Confirmed, ct);
            }

        }
        catch (Exception e)
        {
            await UpdateStatusAsync(booking.Id, BookingStatus.Rejected, ct);
            eventService.ReleaseSeats(booking.EventId);

            logger.LogError($"Неожиданная ошибка при обработке брони {booking.Id}: {e}");
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }
}
