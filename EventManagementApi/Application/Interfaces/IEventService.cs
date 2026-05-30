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
    /// <returns></returns>
    PaginatedResult<Event> GetAll(string? title, DateTime? from, DateTime? to, int page = 1, int pageSize = 10);

    /// <summary>
    /// Получение события по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <returns>Событие</returns>
    Event? Get(Guid id);

    /// <summary>
    /// Создание события
    /// </summary>
    /// <param name="title">Название события</param>
    /// <param name="totalSeats">Общее количество мест на событии</param>
    /// <param name="startAt">Дата начала</param>
    /// <param name="endAt">Дата окончания</param>
    /// <param name="description">Описание события</param>
    /// <returns>Событие</returns>
    Event Create(string title, DateTime? startAt, DateTime? endAt, int totalSeats, string? description = null);

    /// <summary>
    /// Обновление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="title">Название события</param>
    /// <param name="startAt">Дата начала</param>
    /// <param name="endAt">Дата окончания</param>
    /// <param name="totalSeats">Общее количество мест на событии</param>
    /// <param name="description">Описание события</param>
    void Update(Guid id, string title, DateTime? startAt, DateTime? endAt,int totalSeats, string? description = null);


    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    void Remove(Guid id);

    /// <summary>
    /// Попытка резервирования мест
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="count">Количество мест</param>
    /// <returns>true - если удачно</returns>
    bool TryReserveSeats(Guid id, int count = 1);

    /// <summary>
    /// Освобождение мест для резервирования
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="count">Количество мест для освобождения</param>
    void ReleaseSeats(Guid id, int count = 1);
}
