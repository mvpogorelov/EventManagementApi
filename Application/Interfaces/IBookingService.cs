using EventManagmentApi.Application.Enums;
using EventManagmentApi.Application.Models;

namespace EventManagmentApi.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с бронью
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Получение списка брони
    /// </summary>
    /// <param name="eventId">Фильтр по событию</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <returns></returns>
    PaginatedResult<Booking> GetAll(Guid? eventId, int page = 1, int pageSize = 10);

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <returns>Бронь</returns>
    Task<Booking> GetBookingByIdAsync(Guid bookingId);

    /// <summary>
    /// Создание брони
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// <returns>Бронь</returns>
    Task<Booking> CreateBookingAsync(Guid eventId);

    /// <summary>
    /// Обновление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="status">Статус брони</param>
    Task UpdateStatusAsync(Guid id, BookingStatusEnum status);


    /// <summary>
    /// Удаление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    void Remove(Guid id);
}
