using BookingsService.Application.Abstractions.Persistence.Repositories;
using BookingsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingsService.Infrastructure.Persistence.Repositories;

public class OutboxRepository(AppDbContext context) : IOutboxRepository
{
    public async Task<IReadOnlyList<Outbox>> GetUnprocessedMessages(CancellationToken ct) =>
        await context.Outbox
            .Where(m => m.ProcessedAt == null && m.AttemptCount < 5)
            .OrderBy(m => m.CreatedAt)
            .Take(10)
            .ToListAsync(ct);

    public async Task SaveChangesAsync(CancellationToken ct) => await context.SaveChangesAsync(ct);
}
