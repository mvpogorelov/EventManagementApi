using EventManagement.Application.Abstractions.Persistence.Repositories;
using EventManagement.Domain.Common;
using EventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Репозиторий для работы с бронью
/// </summary>
/// <param name="context">Контекст базы данных</param>
public class BookingRepository(AppDbContext context) : IBookingRepository
{
    /// <summary>
    /// Получение списка брони по статусу
    /// </summary>
    /// <param name="status">Фильтр по статусу</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Список брони</returns>
    public async Task<Booking[]> GetByStatusAsync(BookingStatus status, CancellationToken ct = default) =>
        await context.Bookings.Where(b => b.Status == status).ToArrayAsync(ct);

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    public async Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken ct = default) =>
        await context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct);

    /// <summary>
    /// Создание брони
    /// </summary>
    /// <param name="booking">бронь</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    public async Task<Booking> CreateAsync(Booking booking, CancellationToken ct = default)
    {
        await context.Bookings.AddAsync(booking, ct);
        await context.SaveChangesAsync(ct);

        return booking;
    }

    /// <summary>
    /// Обновление брони
    /// </summary>
    /// <param name="booking">бронь</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    public async Task<Booking> UpdateAsync(Booking booking, CancellationToken ct = default)
    {
        context.Bookings.Update(booking);
        await context.SaveChangesAsync(ct);

        return booking;
    }

    /// <summary>
    /// Удаление брони
    /// </summary>
    /// <param name="booking">бронь</param>
    /// <param name="ct">Токен отмены</param>
    public async Task DeleteAsync(Booking booking, CancellationToken ct = default)
    {
        context.Bookings.Remove(booking);
        await context.SaveChangesAsync(ct);
    }
}
