using EventManagement.Application.Abstractions.Persistence.Repositories;
using EventManagement.Application.Abstractions.Services;
using EventManagement.Domain.Common;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;

namespace EventManagement.Application.Services;

/// <summary>
/// Сервис для работы с бронью
/// </summary>
public class BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository) : IBookingService
{
    private static readonly SemaphoreSlim _createSemaphore = new(1, 1);

    /// <summary>
    /// Получение списка брони
    /// </summary>
    /// <param name="status">Фильтр по статусу</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Список брони</returns>
    public async Task<Booking[]> GetByStatusAsync(BookingStatus status, CancellationToken ct = default) =>
        await bookingRepository.GetByStatusAsync(status, ct);

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    public async Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default) =>
        await bookingRepository.GetByIdAsync(bookingId, ct)
        ?? throw new NotFoundException($"Бронь с Id: {bookingId} не найдена");

    /// <summary>
    /// Создание брони
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct = default)
    {
        await _createSemaphore.WaitAsync(ct);

        try
        {
            var @event = await eventRepository.GetByIdAsync(eventId, ct)
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

            return await bookingRepository.CreateAsync(booking, ct);
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
    public async Task UpdateStatusAsync(Guid id, BookingStatus status, CancellationToken ct = default)
    {
        var booking = await bookingRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Бронь с Id: {id} не найдена");

        booking.Status = status;
        booking.ProcessedAt = DateTime.UtcNow;

        await bookingRepository.UpdateAsync(booking, ct);
    }

    /// <summary>
    /// Удаление брони
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <exception cref="NotFoundException">Если бронь не найдена</exception>
    public async Task RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var booking = await bookingRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Бронь с Id: {id} не найдена");

        await bookingRepository.DeleteAsync(booking);
    }
}
