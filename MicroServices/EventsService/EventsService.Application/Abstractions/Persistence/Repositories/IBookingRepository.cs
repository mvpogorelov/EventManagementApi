using EventsService.Domain.Entities;

namespace EventsService.Application.Abstractions.Persistence.Repositories;

/// <summary>
/// Репозиторий для работы с бронью
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken ct = default);

    /// <summary>
    /// Создание брони
    /// </summary>
    /// <param name="booking">бронь</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    Task<Booking> CreateAsync(Booking booking, CancellationToken ct = default);

    /// <summary>
    /// Удаление брони
    /// </summary>
    /// <param name="booking">бронь</param>
    /// <param name="ct">Токен отмены</param>
    Task DeleteAsync(Booking booking, CancellationToken ct = default);

    /// <summary>
    /// Получение всех событий по пользователю и бронированию
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    Task<IReadOnlyList<Booking>> GetAllByUserAndEvent(Guid userId, Guid eventId, CancellationToken ct = default);
}

