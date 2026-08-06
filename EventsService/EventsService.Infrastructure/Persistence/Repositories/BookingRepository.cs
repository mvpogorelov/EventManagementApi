using EventsService.Application.Abstractions.Persistence.Repositories;
using EventsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Репозиторий для работы с бронью
/// </summary>
/// <param name="context">Контекст базы данных</param>
public class BookingRepository(AppDbContext context) : IBookingRepository
{
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
    /// Удаление брони
    /// </summary>
    /// <param name="booking">бронь</param>
    /// <param name="ct">Токен отмены</param>
    public async Task DeleteAsync(Booking booking, CancellationToken ct = default)
    {
        context.Bookings.Remove(booking);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Booking>> GetAllByUserAndEvent(
        Guid userId,
        Guid eventId,
        CancellationToken ct = default) =>
            await context.Bookings
                .Include(b => b.Event)
                .Where(b => b.UserId == userId && b.EventId == eventId)
                .ToListAsync();
}
