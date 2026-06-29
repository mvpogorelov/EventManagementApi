using EventManagement.Domain.Common;
using EventManagement.Domain.Entities;

namespace EventManagement.Application.Abstractions.Services;

/// <summary>
/// Интерфейс сервиса для работы с бронью
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Получение списка брони
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
    Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default);

    /// <summary>
    /// Создание брони
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>
    /// Обновление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="status">Статус брони</param>
    /// <param name="ct">Токен отмены</param>
    Task UpdateStatusAsync(Guid id, BookingStatus status, CancellationToken ct = default);

    /// <summary>
    /// Удаление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    Task RemoveAsync(Guid id, CancellationToken ct = default);
}
