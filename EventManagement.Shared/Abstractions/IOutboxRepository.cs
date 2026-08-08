using EventManagement.Shared.Entities;

namespace EventManagement.Shared.Abstractions;

public interface IOutboxRepository
{
    Task<IReadOnlyList<Outbox>> GetUnprocessedMessages(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
