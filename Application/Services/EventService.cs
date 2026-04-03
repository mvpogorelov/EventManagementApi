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

    /// <summary>
    /// Получение всех событий
    /// </summary>
    /// <returns>Список событий</returns>
    public IReadOnlyList<Event> GetAll() => _events.Values.ToList();

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

        throw new KeyNotFoundException($"Событие с Id: {id} не найдено");
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
    public Event Create(string title, DateTime startAt, DateTime endAt, string? description = null)
    {
        ValidateEventDataAndThrow(title, startAt, endAt);

        var @event = new Event
        {
            Id = _events.Count + 1,
            Title = title,
            StartAt = startAt,
            EndAt = endAt,
            Description = description
        };

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
    /// <exception cref="KeyNotFoundException">Если событие не найдено</exception>
    /// <exception cref="ArgumentException">Если некорректные данные о событии</exception>
    public void Update(int id, string title, DateTime startAt, DateTime endAt, string? description = null)
    {
        ValidateEventDataAndThrow(title, startAt, endAt);

        if (!_events.TryGetValue(id, out var @event))
        {
            throw new KeyNotFoundException($"Событие с Id: {id} не найдено");
        }

        @event.Title = title;
        @event.StartAt = startAt;
        @event.EndAt = endAt;
        @event.Description = description;
    }

    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <exception cref="KeyNotFoundException">Если событие не найдено</exception>
    public void Remove(int id)
    {
        if (!_events.TryGetValue(id, out var @event))
        {
            throw new KeyNotFoundException($"Событие с Id: {id} не найдено");
        }

        _events.Remove(id);
    }

    private void ValidateEventDataAndThrow(string title, DateTime startAt, DateTime endAt, string? description = null)
    {
        if (string.IsNullOrEmpty(title))
        {
            throw new ArgumentException("Название не должно быть пустым");
        }

        if (startAt > endAt)
        {
            throw new ArgumentException("Дата начала не должна быть больше даты окончания");
        }
    }
}
