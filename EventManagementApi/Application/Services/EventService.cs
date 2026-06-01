using EventManagmentApi.Application.Exceptions;
using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Application.Models;
using EventManagmentApi.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EventManagmentApi.Application.Services;

/// <summary>
/// Сервис по работе с событиями
/// </summary>
public class EventService(AppDbContext context) : IEventService
{
    /// <summary>
    /// Получение списка событий
    /// </summary>
    /// <param name="title">Фильтр по названию</param>
    /// <param name="from">С даты</param>
    /// <param name="to">По дату</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <returns>Список событий</returns>
    public PaginatedResult<Event> GetAll(
        string? title = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 10)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException($"Неверный номер страницы: {nameof(page)}");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException($"Неверный размер страницы: {nameof(pageSize)}");
        }

        var events = context.Events.AsQueryable();

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

        var totalItems = events.Count();
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        var items = events
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<Event>(items, page, items.Count, totalItems, totalPages);
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
    /// <param name="title">Название события</param>
    /// <param name="totalSeats">Общее количество мест на событии</param>
    /// <param name="startAt">Дата начала</param>
    /// <param name="endAt">Дата окончания</param>
    /// <param name="description">Описание события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    /// <exception cref="ArgumentException">Не корректные аргументы</exception>
    public async Task<Event> CreateAsync(string title, DateTime? startAt, DateTime? endAt, int totalSeats, string? description, CancellationToken ct = default)
    {
        ValidateEventDataAndThrow(title, startAt, endAt, totalSeats);

        var @event = new Event(title, startAt.Value, endAt.Value, totalSeats, description);

        await context.Events.AddAsync(@event, ct);
        await context.SaveChangesAsync();

        return @event;
    }

    /// <summary>
    /// Обновление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="title">Название события</param>
    /// <param name="startAt">Дата начала</param>
    /// <param name="endAt">Дата окончания</param>
    /// <param name="totalSeats">Общее количество мест на событии</param>
    /// <param name="description">Описание события</param>
    /// <param name="ct">Токен отмены</param>
    /// <exception cref="NotFoundException">Если событие не найдено</exception>
    /// <exception cref="ArgumentException">Если некорректные данные о событии</exception>
    public async Task UpdateAsync(Guid id, string title, DateTime? startAt, DateTime? endAt, int totalSeats, string? description, CancellationToken ct = default)
    {
        ValidateEventDataAndThrow(title, startAt, endAt, totalSeats);

        var @event = await GetByIdAsync(id, ct) ?? throw new NotFoundException($"Событие с Id: {id} не найдено");

        @event.Title = title;
        @event.StartAt = startAt.Value;
        @event.EndAt = endAt.Value;
        @event.Description = description;
        @event.TotalSeats = totalSeats;

        await context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <exception cref="NotFoundException">Если событие не найдено</exception>
    public async Task RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var @event = await GetByIdAsync(id, ct) ?? throw new NotFoundException($"Событие с Id: {id} не найдено");

        context.Remove(@event);
        await context.SaveChangesAsync(ct);
    }
    
    /// <summary>
    /// Удаление всех событий
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    public async Task RemoveAllAsync(CancellationToken ct = default)
    {
        var allEvents = await context.Events.ToListAsync(ct);

        context.Events.RemoveRange(allEvents);
        await context.SaveChangesAsync();
    }

    private void ValidateEventDataAndThrow(string title, DateTime? startAt, DateTime? endAt, int totalSeats, string? description = null)
    {
        if (string.IsNullOrEmpty(title))
        {
            throw new ValidationException($"Название должно быть заполнено: {nameof(title)}");
        }

        if (!startAt.HasValue)
        {
            throw new ValidationException($"Дата начала должна быть заполнена: {nameof(startAt)}");
        }
        
        if (!endAt.HasValue)
        {
            throw new ValidationException($"Дата окончания должна быть заполнена: {nameof(endAt)}");
        }

        if (startAt > endAt)
        {
            throw new ValidationException("Дата начала не должна быть больше даты окончания");
        }

        if (totalSeats <= 0)
        {
            throw new ValidationException($"Общее количество мест должно быть больше нуля: {nameof(totalSeats)}");
        }
    }
}
