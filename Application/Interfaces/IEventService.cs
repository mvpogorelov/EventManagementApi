using EventManagmentApi.Models;

namespace EventManagmentApi.Application.Interfaces;

public interface IEventService
{
    IReadOnlyList<Event> GetAll();
    Event Get(int id);
    void Add(Event @event);
    void Update(Event @event);
    void Remove(int id);
}
