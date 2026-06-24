using EventManagmentApi.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagmentApi.Application.Interfaces;

public interface IEventRepository
{
    /// <summary>
    /// Получение списка событий
    /// </summary>
    /// <param name="title">Фильтр по названию</param>
    /// <param name="from">С даты</param>
    /// <param name="to">По дату</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Список событий</returns>
    Task<PaginatedResult<Event>> GetPaginatedAsync(
        string? title = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default);

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
    /// <param name="event">Событие</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    Task<Event> CreateAsync(Event @event, CancellationToken ct = default);
    
    /// <summary>
    /// Обновление события
    /// </summary>
    /// <param name="event">Событие</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    Task<Event> UpdateAsync(Event @event, CancellationToken ct = default);
   
    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="event">Событие</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    Task DeleteAsync(Event @event, CancellationToken ct = default);

    /// <summary>
    /// Удаление всех событий
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    Task DeleteAllAsync(CancellationToken ct = default);
}
