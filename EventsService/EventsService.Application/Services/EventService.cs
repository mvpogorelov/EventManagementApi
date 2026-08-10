using EventsService.Application.Abstractions.Persistence.Repositories;
using EventsService.Application.Abstractions.Services;
using EventsService.Application.DTOs;
using EventsService.Domain.Entities;
using EventsService.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EventsService.Application.Services;

/// <summary>
/// Сервис по работе с событиями
/// </summary>
public class EventService : IEventService
{
    private readonly IDatabase _redisDb;
    private readonly IEventRepository _repository;
    private readonly ILogger<EventService> _logger;
    private const string EventsTop10Key = "events:top10";

    public EventService(
        IConnectionMultiplexer multiplexer,
        IEventRepository repository,
        ILogger<EventService> logger)
    {
        _redisDb = multiplexer.GetDatabase();
        _repository = repository;
        _logger = logger;
    }

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
    public async Task<PaginatedResult<Event>> GetAllAsync(
        string? title = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException($"Неверный номер страницы: {nameof(page)}");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException($"Неверный размер страницы: {nameof(pageSize)}");
        }

        return await _repository.GetPaginatedAsync(title, from, to, page, pageSize, ct);
    }

    /// <summary>
    /// Получение события по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    public async Task<Event> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cacheKey = $"event:{id}";
        var cached = await GetCachedStringAsync<Event>(cacheKey);

        if (cached is not null)
        {
            return cached;
        }

        var @event = await _repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Cобытие не найдено: {id}");

        await SetCachedStringAsync(cacheKey, @event, 5 * 60);

        return @event;
    }

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

        return await _repository.CreateAsync(@event);
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

        var @event = await _repository.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Событие с Id: {id} не найдено");

        @event.Title = title;
        @event.StartAt = startAt.Value;
        @event.EndAt = endAt.Value;
        @event.Description = description;
        @event.TotalSeats = totalSeats;

        await _repository.UpdateAsync(@event);
    }

    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <exception cref="NotFoundException">Если событие не найдено</exception>
    public async Task RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var @event = await _repository.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Событие с Id: {id} не найдено");

        await _repository.DeleteAsync(@event);
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

    private async Task<T?> GetCachedStringAsync<T>(string cacheKey)
    {
        try
        {
            var cached = await _redisDb.StringGetAsync(cacheKey);

            if (cached.HasValue && !cached.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<T>(cached.ToString());
            }
        }
        catch (RedisTimeoutException e)
        {
            _logger.LogWarning(e, "Redis timeout");
        }
        catch (RedisConnectionException e)
        {
            _logger.LogError(e, "Redis нет связи");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Redis ошибка");
        }

        return default;
    }

    private async Task SetCachedStringAsync<T>(string cacheKey, T obj, int ttlSeconds)
    {
        try
        {
            byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(obj);

            await _redisDb.StringSetAsync(cacheKey, jsonBytes, TimeSpan.FromSeconds(ttlSeconds));
        }
        catch (RedisServerException e) when (e.Message.Contains("OOM"))
        {
            _logger.LogCritical(e, "Redis переполнен");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Redis ошибка сохранения ключа {CacheKey}", cacheKey);
        }
    }

    public async Task<IReadOnlyList<Event>> GetTop(int count, CancellationToken ct = default)
    {
        var cached = await GetCachedStringAsync<IReadOnlyList<Event>>(EventsTop10Key);

        if (cached is not null)
        {
            return cached;
        }

        var events = await _repository.GetTop(count, ct);

        await SetCachedStringAsync(EventsTop10Key, events, 10 * 60);

        return events;
    }
}