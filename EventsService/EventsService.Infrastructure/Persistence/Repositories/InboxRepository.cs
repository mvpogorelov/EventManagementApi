using EventsService.Application.Abstractions.Persistence.Repositories;
using EventsService.Domain.Common;
using EventsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsService.Infrastructure.Persistence.Repositories;

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

    public async Task<IReadOnlyList<Inbox>> GetByUserAndEvent(Guid userId, Guid eventId, CancellationToken ct = default) =>
        await context.Inbox
            .Include(i => i.Event)
            .Where(i => i.Status == InboxStatus.Processed && i.UserId == userId && i.EventId == eventId)
            .ToListAsync(ct);

    public async Task<Inbox> UpdateAsync(Inbox inbox, CancellationToken ct = default)
    {
        context.Inbox.Update(inbox);
        await context.SaveChangesAsync(ct);

        return inbox;
    }
}
