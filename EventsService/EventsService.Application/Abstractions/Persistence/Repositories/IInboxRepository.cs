using EventsService.Domain.Entities;

namespace EventsService.Application.Abstractions.Persistence.Repositories;

public interface IInboxRepository
{
    Task<Inbox?> GetByMessageAsync(string topic, string messageKey, string? messageType = null, CancellationToken ct = default);
    Task<Inbox?> GetByMessageAsync(string topic, Guid messageKey, string? messageType = null, CancellationToken ct = default);
    Task<Inbox> CreateAsync(Inbox inbox, CancellationToken ct = default);
    Task<Inbox> UpdateAsync(Inbox inbox, CancellationToken ct = default);
    Task<IReadOnlyList<Inbox>> GetByUserAndEvent(Guid userId, Guid eventId, CancellationToken ct = default);
}
