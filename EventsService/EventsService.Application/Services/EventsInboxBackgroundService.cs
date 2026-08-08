using EventManagement.Shared.Entities;
using EventManagement.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventsService.Application.Services;

public class EventsInboxBackgroundService : InboxBackgroundService
{
    public EventsInboxBackgroundService(ILogger<EventsInboxBackgroundService> logger, IServiceScopeFactory scopeFactory)
        : base(logger, scopeFactory)
    {
    }

    protected override string Topic => throw new NotImplementedException();

    protected override Task ProcessBusinessLogicAsync(Inbox inbox, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}