using EventManagmentApi.Application.Models;

namespace EventManagmentApi.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с событиями
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Получение всех событий
    /// </summary>
    /// <param name="title">Фильтр по названию</param>
    /// <param name="from">С даты</param>
    /// <param name="to">По дату</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns></returns>
    Task<PaginatedResult<Event>> GetAllAsync(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10, CancellationToken ct = default);

    /// <summary>
    /// Получение события по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Создание события
    /// </summary>
    /// <param name="title">Название события</param>
    /// <param name="totalSeats">Общее количество мест на событии</param>
    /// <param name="startAt">Дата начала</param>
    /// <param name="endAt">Дата окончания</param>
    /// <param name="description">Описание события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    Task<Event> CreateAsync(string title, DateTime? startAt, DateTime? endAt, int totalSeats, string? description = null, CancellationToken ct = default);

    /// <summary>
    /// Обновление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="title">Название события</param>
    /// <param name="startAt">Дата начала</param>
    /// <param name="endAt">Дата окончания</param>
    /// <param name="totalSeats">Общее количество мест на событии</param>
    /// <param name="description">Описание события</param>
    /// <param name="ct">Токен отмены</param>
    Task UpdateAsync(Guid id, string title, DateTime? startAt, DateTime? endAt,int totalSeats, string? description = null, CancellationToken ct = default);


    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    Task RemoveAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Удаление всех событий
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    Task RemoveAllAsync(CancellationToken ct = default);
}
