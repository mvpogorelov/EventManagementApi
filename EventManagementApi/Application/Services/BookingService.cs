using EventManagmentApi.Application.Enums;
using EventManagmentApi.Application.Exceptions;
using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Application.Models;
using EventManagmentApi.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventManagmentApi.Application.Services;

/// <summary>
/// Сервис для работы с бронью
/// </summary>
public class BookingService(AppDbContext context) : IBookingService
{
    private static readonly SemaphoreSlim _createSemaphore = new(1, 1);

    /// <summary>
    /// Получение списка брони
    /// </summary>
    /// <param name="status">Фильтр по статусу</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Список брони</returns>
    public async Task<Booking[]> GetByStatusAsync(BookingStatus status, CancellationToken ct) =>
        await context.Bookings.Where(b => b.Status == status).ToArrayAsync(ct);

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    public async Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct) =>
        await context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct)
        ?? throw new NotFoundException($"Бронь с Id: {bookingId} не найдена");

    /// <summary>
    /// Создание брони
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct)
    {
        await _createSemaphore.WaitAsync(ct);

        try
        {
            var @event = await context.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct)
                ?? throw new NotFoundException($"Событие не найдено: {eventId}");

            if (!@event.TryReserveSeats())
            {
                throw new NoAvailableSeatsException($"Нет доступных мест для события: {eventId}");
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await context.Bookings.AddAsync(booking, ct);
            await context.SaveChangesAsync(ct);

            return booking;
        }
        finally
        {
            _createSemaphore.Release();
        }
    }

    /// <summary>
    /// Обновление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="status">Статус брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <exception cref="NotFoundException">Если бронь не найдена</exception>
    public async Task UpdateStatusAsync(Guid id, BookingStatus status, CancellationToken ct)
    {
        var booking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new NotFoundException($"Бронь с Id: {id} не найдена");

        booking.Status = status;
        booking.ProcessedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Удаление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <exception cref="NotFoundException">Если бронь не найдена</exception>
    public async Task RemoveAsync(Guid id, CancellationToken ct)
    {
        var booking = await context.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new NotFoundException($"Бронь с Id: {id} не найдена");

        context.Remove(booking);
        await context.SaveChangesAsync(ct);
    }
}
