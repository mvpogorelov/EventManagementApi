using EventManagement.Shared.Abstractions;
using EventManagement.Shared.Entities;

namespace BookingsService.Infrastructure.Persistence.Repositories;

public class InboxRepository(AppDbContext context) : IInboxRepository
{
    public async Task CreateAsync(Inbox inbox, CancellationToken ct = default)
    {
        await context.Inbox.AddAsync(inbox, ct);
        await context.SaveChangesAsync(ct);
    }
}