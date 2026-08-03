using BookingsService.Domain.Entities;
using EventManagement.Contracts.Common;

namespace BookingsService.Application.Abstractions.Services;

/// <summary>
/// Интерфейс сервиса для работы с бронью
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    Task<Booking> GetBookingByIdAsync(Guid bookingId, Guid userId, UserRole userRole, CancellationToken ct = default);

    /// <summary>
    /// Создание брони
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    Task<Booking> CreateBookingAsync(Guid eventId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Удаление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    Task RemoveAsync(Guid id, Guid userId, UserRole userRole, CancellationToken ct = default);

    /// <summary>
    /// Отмена брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="currentUserId">Идентификатор пользователя</param>
    /// <param name="ct">Токен отмены</param>
    Task CancelAsync(Guid id, Guid userId, UserRole userRole, CancellationToken ct = default);
}

