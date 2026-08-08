using EventManagement.Shared.Abstractions;
using EventManagement.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsService.Infrastructure.Persistence.Repositories;

public class InboxRepository(AppDbContext context) : IInboxRepository
{
    public async Task CreateAsync(Inbox inbox, CancellationToken ct = default)
    {
        await context.Inbox.AddAsync(inbox, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Inbox>> GetUnprocessedMessages(string topic, CancellationToken ct) =>
        await context.Inbox
            .Where(m => m.Topic == topic && m.ProcessedAt == null && m.AttemptCount < 5)
            .OrderBy(m => m.CreatedAt)
            .Take(10)
            .ToListAsync(ct);

    public async Task SaveChangesAsync(CancellationToken ct) => await context.SaveChangesAsync(ct);
}
