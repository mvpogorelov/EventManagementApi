using BookingsService.Domain.Entities;

namespace BookingsService.Application.Abstractions.Persistence.Repositories;

public interface IOutboxRepository
{
    Task<IReadOnlyList<Outbox>> GetUnprocessedMessages(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
