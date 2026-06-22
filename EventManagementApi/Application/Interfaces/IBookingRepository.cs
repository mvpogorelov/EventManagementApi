using EventManagmentApi.Application.Enums;
using EventManagmentApi.Application.Models;

namespace EventManagmentApi.Application.Interfaces;

/// <summary>
/// Репозиторий для работы с бронью
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Получение списка брони по статусу
    /// </summary>
    /// <param name="status">Фильтр по статусу</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Список брони</returns>
    Task<Booking[]> GetByStatusAsync(BookingStatus status, CancellationToken ct = default);

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
    /// Обновление брони
    /// </summary>
    /// <param name="booking">бронь</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    Task<Booking> UpdateAsync(Booking booking, CancellationToken ct = default);

    /// <summary>
    /// Удаление брони
    /// </summary>
    /// <param name="booking">бронь</param>
    /// <param name="ct">Токен отмены</param>
    Task DeleteAsync(Booking booking, CancellationToken ct = default);
}
