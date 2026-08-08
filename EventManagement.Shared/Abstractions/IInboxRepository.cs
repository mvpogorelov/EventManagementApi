using EventManagement.Shared.Entities;

namespace EventManagement.Shared.Abstractions;

public interface IInboxRepository
{
    Task CreateAsync(Inbox inbox, CancellationToken ct);
    Task<IReadOnlyList<Inbox>> GetUnprocessedMessages(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
