using EventManagmentApi.Application.Exceptions;
using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Models;
using EventManagmentApi.Presentation.Dto;

namespace EventManagmentApi.Application.Services;

/// <summary>
/// Сервис по работе с событиями
/// </summary>
public class EventService : IEventService
{
    private static Dictionary<int, Event> _events = new();
    private static int _lastId = 1;

    /// <summary>
    /// Получение всех событий
    /// </summary>
    /// <param name="title">Фильтр по названию</param>
    /// <param name="from">С даты</param>
    /// <param name="to">По дату</param>
    /// <returns>Список событий</returns>
    public IReadOnlyList<Event> GetAll(string? title, DateTime? from, DateTime? to)
    {
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

        return events.ToList();
    }

    /// <summary>
    /// Получение события по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <returns>Событие</returns>
    public Event Get(int id)
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
    /// <param name="startAt">Дата начала</param>
    /// <param name="endAt">Дата окончания</param>
    /// <param name="description">Описание события</param>
    /// <returns>Событие</returns>
    /// <exception cref="ArgumentException">Не корректные аргументы</exception>
    public Event Create(string title, DateTime? startAt, DateTime? endAt, string? description = null)
    {
        ValidateEventDataAndThrow(title, startAt, endAt);

#pragma warning disable CS8629 // Тип значения, допускающего NULL, может быть NULL.
        var @event = new Event
        {
            Id = _lastId++,
            Title = title,
            StartAt = startAt.Value,
            EndAt = endAt.Value,
            Description = description
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
    /// <param name="description">Описание события</param>
    /// <exception cref="NotFoundException">Если событие не найдено</exception>
    /// <exception cref="ArgumentException">Если некорректные данные о событии</exception>
    public void Update(int id, string title, DateTime? startAt, DateTime? endAt, string? description = null)
    {
        ValidateEventDataAndThrow(title, startAt, endAt);

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
    }

    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <exception cref="NotFoundException">Если событие не найдено</exception>
    public void Remove(int id)
    {
        if (!_events.TryGetValue(id, out var @event))
        {
            throw new NotFoundException($"Событие с Id: {id} не найдено");
        }

        _events.Remove(id);
    }

    private void ValidateEventDataAndThrow(string title, DateTime? startAt, DateTime? endAt, string? description = null)
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
            throw new ArgumentException($"Дата начала не должна быть больше даты окончания");
        }
    }
}
