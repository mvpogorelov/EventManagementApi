using EventManagement.Shared.Abstractions;
using EventManagement.Shared.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventManagement.Shared.Services;

public abstract class InboxBackgroundService(
    ILogger logger,
    IServiceScopeFactory scopeFactory)
        : BackgroundService
{
    private const int PollingInterval = 10000;
    private const int MaxCriticalExeptions = 10;
    protected abstract string Topic { get; }
    
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("InboxBackgroundService для топика {Topic} запущен", Topic);

        int errorCount = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessInboxMessagesAsync(stoppingToken);
                await Task.Delay(PollingInterval, stoppingToken);
                errorCount = 0;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception e)
            {
                errorCount++;
                logger.LogCritical(e, $"Inbox критическая ошибка");

                if (errorCount > MaxCriticalExeptions)
                {
                    throw;
                }

                var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, errorCount), 60));

                await Task.Delay(delay, stoppingToken);
            }
        }

        logger.LogInformation("InboxBackgroundService для топика {Topic} остановлен", Topic);
    }

    protected async Task ProcessInboxMessagesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var inboxRepository = scope.ServiceProvider.GetRequiredService<IInboxRepository>();
        var messages = await inboxRepository.GetUnprocessedMessages(Topic, ct);

        if (!messages.Any())
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                message.AttemptCount++;

                await ProcessBusinessLogicAsync(message, ct);

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception e)
            {
                message.Error = e.Message;
            }
        }

        await inboxRepository.SaveChangesAsync(ct);
    }

    protected abstract Task ProcessBusinessLogicAsync(Inbox inbox, CancellationToken ct);
}
