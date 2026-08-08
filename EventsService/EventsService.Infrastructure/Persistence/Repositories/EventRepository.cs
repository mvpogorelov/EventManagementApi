using EventManagement.Shared.Entities;
using EventsService.Application.Abstractions.Persistence.Repositories;
using EventsService.Application.DTOs;
using EventsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsService.Infrastructure.Persistence.Repositories;

public class EventRepository(AppDbContext context) : IEventRepository
{
    /// <summary>
    /// Получение списка событий
    /// </summary>
    /// <param name="title">Фильтр по названию</param>
    /// <param name="from">С даты</param>
    /// <param name="to">По дату</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Список событий</returns>
    public async Task<PaginatedResult<Event>> GetPaginatedAsync(
        string? title = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var events = context.Events.AsNoTracking();

        if (!string.IsNullOrEmpty(title))
        {
            events = events.Where(e => e.Title.ToLower().Contains(title.ToLower()));
        }

        if (from.HasValue)
        {
            events = events.Where(e => e.StartAt >= from);
        }

        if (to.HasValue)
        {
            events = events.Where(e => e.EndAt <= to);
        }

        var totalItems = await events.CountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        var items = await events
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedResult<Event>(items, page, pageSize, totalItems, totalPages);
    }

    /// <summary>
    /// Получение события по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

    /// <summary>
    /// Создание события
    /// </summary>
    /// <param name="event">Событие</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    public async Task<Event> CreateAsync(Event @event, CancellationToken ct = default)
    {
        await context.Events.AddAsync(@event, ct);
        await context.SaveChangesAsync(ct);

        return @event;
    }

    /// <summary>
    /// Обновление события
    /// </summary>
    /// <param name="event">Событие</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    public async Task<Event> UpdateAsync(Event @event, CancellationToken ct = default)
    {
        context.Events.Update(@event);
        await context.SaveChangesAsync(ct);

        return @event;
    }

    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="event">Событие</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    public async Task DeleteAsync(Event @event, CancellationToken ct = default)
    {
        context.Events.Remove(@event);
        await context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Удаление всех событий
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    public async Task DeleteAllAsync(CancellationToken ct = default)
    {
        var allEvents = await context.Events.ToListAsync(ct);

        context.Events.RemoveRange(allEvents);
        await context.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default) => await context.SaveChangesAsync(ct);

    public async Task<Inbox?> GetInboxByIdAsync(int id, CancellationToken ct = default) =>
        await context.Inbox.FirstOrDefaultAsync(i => i.Id == id);

    public async Task<Inbox?> GetInboxByMessageTypeAndBookingId(
        string topic,
        string messageType,
        Guid bookingId,
        CancellationToken ct = default) =>
            await context.Inbox
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    i => i.ProcessedAt != null
                        && i.Topic == topic
                        && i.MessageType == messageType
                        && i.BookingId == bookingId,
                    ct
                );

    public async Task AddOutboxAsync(Outbox outbox, CancellationToken ct = default) =>
        await context.Outbox.AddAsync(outbox);
}
