using EventManagmentApi.Models;

namespace EventManagmentApi.Application.Interfaces;

public interface IEventService
{
    IReadOnlyList<Event> GetAll();
    Event Get(int id);
    Event Create(string title, DateTime startAt, DateTime endAt, string? description = null);
    void Update(int id, string title, DateTime startAt, DateTime endAt, string? description = null);
    void Remove(int id);
}
