using EventManagmentApi.Application.Enums;
using EventManagmentApi.Application.Models;
using System.Collections.ObjectModel;

namespace EventManagmentApi.Application.Interfaces;

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
    Task<IReadOnlyList<Booking>> GetByStatusAsync(BookingStatusEnum status, CancellationToken ct);

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct);

    /// <summary>
    /// Создание брони
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct);

    /// <summary>
    /// Обновление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="status">Статус брони</param>
    /// <param name="ct">Токен отмены</param>
    Task UpdateStatusAsync(Guid id, BookingStatusEnum status, CancellationToken ct);

    /// <summary>
    /// Удаление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    void Remove(Guid id);
}
