using EventManagmentApi.Application.Exceptions;
using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Application.Models;

namespace EventManagmentApi.Application.Services;

/// <summary>
/// Сервис по работе с событиями
/// </summary>
public class EventService : IEventService
{
    private static Dictionary<Guid, Event> _events = new();

    /// <summary>
    /// Получение списка событий
    /// </summary>
    /// <param name="title">Фильтр по названию</param>
    /// <param name="from">С даты</param>
    /// <param name="to">По дату</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <returns>Список событий</returns>
    public PaginatedResult<Event> GetAll(
        string? title = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 10)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException($"Неверный номер страницы: {nameof(page)}");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException($"Неверный размер страницы: {nameof(pageSize)}");
        }

        var events = _events.Values as IEnumerable<Event>;

        if (!string.IsNullOrEmpty(title))
        {
            events = events.Where(e => e.Title.ToLower().Contains(title.ToLower()));
        }
        
        if (from.HasValue)
        {
            events = events.Where(e => e.StartAt >= from);
        }
        
        if (to.HasValue)
        {
            events = events.Where(e => e.EndAt <= to);
        }

        var totalItems = events.Count();
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        var items = events
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<Event>(items, page, items.Count, totalItems, totalPages);
    }

    /// <summary>
    /// Получение события по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <returns>Событие</returns>
    public Event Get(Guid id)
    {
        if (_events.TryGetValue(id, out var @event))
        {
            return @event;
        }

        throw new NotFoundException($"Событие с Id: {id} не найдено");
    }

    /// <summary>
    /// Создание события
    /// </summary>
    /// <param name="title">Название события</param>
    /// <param name="totalSeats">Общее количество мест на событии</param>
    /// <param name="startAt">Дата начала</param>
    /// <param name="endAt">Дата окончания</param>
    /// <param name="description">Описание события</param>
    /// <returns>Событие</returns>
    /// <exception cref="ArgumentException">Не корректные аргументы</exception>
    public Event Create(string title, DateTime? startAt, DateTime? endAt, int totalSeats, string? description = null)
    {
        ValidateEventDataAndThrow(title, startAt, endAt, totalSeats);

#pragma warning disable CS8629 // Тип значения, допускающего NULL, может быть NULL.
        var @event = new Event
        {
            Id = Guid.NewGuid(),
            Title = title,
            StartAt = startAt.Value,
            EndAt = endAt.Value,
            Description = description,
            TotalSeats = totalSeats,
            AvailableSeats = totalSeats
        };
#pragma warning restore CS8629 // Тип значения, допускающего NULL, может быть NULL.

        _events.Add(@event.Id, @event);

        return @event;
    }

    /// <summary>
    /// Обновление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="title">Название события</param>
    /// <param name="startAt">Дата начала</param>
    /// <param name="endAt">Дата окончания</param>
    /// <param name="totalSeats">Общее количество мест на событии</param>
    /// <param name="description">Описание события</param>
    /// <exception cref="NotFoundException">Если событие не найдено</exception>
    /// <exception cref="ArgumentException">Если некорректные данные о событии</exception>
    public void Update(Guid id, string title, DateTime? startAt, DateTime? endAt, int totalSeats, string? description = null)
    {
        ValidateEventDataAndThrow(title, startAt, endAt, totalSeats);

        if (!_events.TryGetValue(id, out var @event))
        {
            throw new NotFoundException($"Событие с Id: {id} не найдено");
        }

        @event.Title = title;
#pragma warning disable CS8629 // Тип значения, допускающего NULL, может быть NULL.
        @event.StartAt = startAt.Value;
        @event.EndAt = endAt.Value;
#pragma warning restore CS8629 // Тип значения, допускающего NULL, может быть NULL.
        @event.Description = description;
        @event.TotalSeats = totalSeats;
        @event.AvailableSeats = totalSeats;
    }

    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <exception cref="NotFoundException">Если событие не найдено</exception>
    public void Remove(Guid id)
    {
        if (!_events.TryGetValue(id, out var @event))
        {
            throw new NotFoundException($"Событие с Id: {id} не найдено");
        }

        _events.Remove(id);
    }

    private void ValidateEventDataAndThrow(string title, DateTime? startAt, DateTime? endAt, int totalSeats, string? description = null)
    {
        if (string.IsNullOrEmpty(title))
        {
            throw new ArgumentException($"Название должно быть заполнено: {nameof(title)}");
        }

        if (!startAt.HasValue)
        {
            throw new ArgumentException($"Дата начала должна быть заполнена: {nameof(startAt)}");
        }
        
        if (!endAt.HasValue)
        {
            throw new ArgumentException($"Дата окончания должна быть заполнена: {nameof(endAt)}");
        }

        if (startAt > endAt)
        {
            throw new ArgumentException("Дата начала не должна быть больше даты окончания");
        }

        if (totalSeats <= 0)
        {
            throw new ArgumentException($"Общее количество мест должно быть больше нуля: {nameof(totalSeats)}");
        }
    }

    /// <summary>
    /// Инициализация данных
    /// </summary>
    /// <param name="events"></param>
    public void InitData(Dictionary<Guid, Event> events)
    {
        _events = events;
    }
}
