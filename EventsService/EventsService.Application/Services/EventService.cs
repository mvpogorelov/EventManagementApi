using EventManagement.Contracts.Kafka;
using EventManagement.Shared.Abstractions;
using EventManagement.Shared.Models;
using EventsService.Application.Abstractions.Persistence.Repositories;
using EventsService.Application.Abstractions.Services;
using EventsService.Application.DTOs;
using EventsService.Domain.Entities;
using EventsService.Domain.Exceptions;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace EventsService.Application.Services;

/// <summary>
/// Сервис по работе с событиями
/// </summary>
public class EventService(
    IEventRepository eventRepository,
    IInboxRepository inboxRepository,
    IKafkaProducerService kafkaProducer,
    IOptions<KafkaSettings> kafkaSettings)
        : IEventService
{
    private const int UserBookingLimit = 10;
    private static readonly SemaphoreSlim _createSemaphore = new(1, 1);

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

        return await eventRepository.GetPaginatedAsync(title, from, to, page, pageSize, ct);
    }

    /// <summary>
    /// Получение события по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    public async Task<Event> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var @event = await eventRepository.GetByIdAsync(id, ct);

        if (@event is null)
        {
            throw new NotFoundException($"Cобытие не найдено: {id}");
        }

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

        return await eventRepository.CreateAsync(@event);
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

        var @event = await eventRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Событие с Id: {id} не найдено");

        @event.Title = title;
        @event.StartAt = startAt.Value;
        @event.EndAt = endAt.Value;
        @event.Description = description;
        @event.TotalSeats = totalSeats;

        await eventRepository.UpdateAsync(@event);
    }

    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <exception cref="NotFoundException">Если событие не найдено</exception>
    public async Task RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var @event = await eventRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Событие с Id: {id} не найдено");

        await eventRepository.DeleteAsync(@event);
    }

    /// <summary>
    /// Удаление всех событий
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    public async Task RemoveAllAsync(CancellationToken ct = default) => await eventRepository.DeleteAllAsync(ct);

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

    public async Task ApproveBookingAsync(
        Guid eventId,
        Guid userId,
        Guid bookingId,
        int seats,
        CancellationToken ct = default)
    {
        await _createSemaphore.WaitAsync(ct);

        try
        {
            var @event = await eventRepository.GetByIdAsync(eventId, ct)
                ?? throw new NotFoundException($"Событие не найдено: {eventId}");

            if (@event.StartAt < DateTime.UtcNow)
            {
                throw new PastEventBookingException("Событие уже началось");
            }

            var inboxes = await inboxRepository.GetByUserAndEvent(userId, eventId, ct);

            if (inboxes.Count(b => b.Event?.StartAt >= DateTime.UtcNow) >= UserBookingLimit)
            {
                throw new BookingLimitException($"Превышен лимит активных броней: {UserBookingLimit}");
            }

            if (!@event.TryReserveSeats(seats))
            {
                throw new NoAvailableSeatsException($"Нет доступных мест для события: {eventId}");
            }

            await eventRepository.UpdateAsync(@event, ct);
        }
        catch (Exception e)
        {
            await kafkaProducer.PublishAsync(
                kafkaSettings.Value.Topics.Events,
                eventId.ToString(),
                new BookingRejected { BookingId = bookingId, Reason = e.Message }, ct);
        }
        finally
        {
            _createSemaphore.Release();
        }
    }
}