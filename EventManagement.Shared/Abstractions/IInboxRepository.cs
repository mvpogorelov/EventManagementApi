using EventManagement.Shared.Entities;

namespace EventManagement.Shared.Abstractions;

public interface IInboxRepository
{
    Task CreateAsync(Inbox inbox, CancellationToken ct);
}
