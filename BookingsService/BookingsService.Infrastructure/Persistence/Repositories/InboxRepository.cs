using BookingsService.Application.Abstractions.Persistence.Repositories;
using BookingsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingsService.Infrastructure.Persistence.Repositories;

public class InboxRepository(AppDbContext context) : IInboxRepository
{
    public async Task<Inbox> CreateAsync(Inbox inbox, CancellationToken ct = default)
    {
        await context.Inbox.AddAsync(inbox, ct);
        await context.SaveChangesAsync(ct);

        return inbox;
    }

    public async Task<Inbox?> GetByMessageAsync(string topic,
        string messageKey,
        string? messageType = null,
        CancellationToken ct = default) =>
            await context.Inbox
                .FirstOrDefaultAsync(i => i.Topic == topic && i.MessageKey == messageKey && i.MessageType == messageType, ct);

    public async Task<Inbox?> GetByMessageAsync(string topic,
        Guid messageKey,
        string? messageType = null,
        CancellationToken ct = default) =>
            await GetByMessageAsync(topic, messageKey.ToString(), messageType, ct);
    public async Task<Inbox> UpdateAsync(Inbox inbox, CancellationToken ct = default)
    {
        context.Inbox.Update(inbox);
        await context.SaveChangesAsync(ct);

        return inbox;
    }
}