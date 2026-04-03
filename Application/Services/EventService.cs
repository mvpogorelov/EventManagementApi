using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Models;

namespace EventManagmentApi.Application.Services;

/// <summary>
/// Сервис по работе с событиями
/// </summary>
public class EventService : IEventService
{
    private readonly Dictionary<int, Event> _events = new();

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
    public Event Get(int id) => _events[id];

    /// <summary>
    /// Добавление события
    /// </summary>
    /// <param name="event">Событие</param>
    /// <exception cref="InvalidOperationException">Если событие уже существует</exception>
    public void Add(Event @event)
    {
        if (_events.ContainsKey(@event.Id))
        {
            throw new InvalidOperationException($"Событие с Id: {@event.Id} уже существует");
        }

        _events.Add(@event.Id, @event);
    }

    /// <summary>
    /// Обновление события
    /// </summary>
    /// <param name="event">Событие</param>
    /// <exception cref="InvalidOperationException">Если событие не найдено</exception>
    public void Update(Event @event)
    {
        var @event2Update = Get(@event.Id);

        if (@event2Update is null)
        {
            throw new InvalidOperationException($"Событие с Id: {@event.Id} не найдено");
        }

        @event2Update.Title = @event.Title;
        @event2Update.StartAt = @event.StartAt;
        @event2Update.EndAt = @event.EndAt;
        @event2Update.Description = @event.Description;
    }

    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    public void Remove(int id) => _events.Remove(id);
}
