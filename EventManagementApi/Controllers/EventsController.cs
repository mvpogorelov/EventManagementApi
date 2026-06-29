using EventManagement.Application.Abstractions.Services;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using EventManagement.Presentation.Contracts;
using EventManagement.Presentation.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventManagement.Presentation.Controllers;

/// <summary>
/// Контроллер обработки событий
/// </summary>
/// <param name="eventService"></param>
/// <param name="bookingService"></param>
[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService, IBookingService bookingService) : ControllerBase
{
    /// <summary>
    /// Получение списка событий
    /// </summary>
    /// <param name="title">Фильтр по названию</param>
    /// <param name="from">С даты</param>
    /// <param name="to">По дату</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <returns>Cписок событий</returns>
    /// <response code="200">Список событий</response>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PaginatedResultDto<IReadOnlyList<EventInfoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status400BadRequest)]
    public async Task<PaginatedResultDto<IReadOnlyList<EventInfoDto>>> GetAll(
        [FromQuery] string? title,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await eventService.GetAllAsync(title, from, to, page, pageSize);

        return new PaginatedResultDto<IReadOnlyList<EventInfoDto>>
        {
            Data = result.Items
                .Select(e => new EventInfoDto
                {
                    AvailableSeats = e.AvailableSeats,
                    Description = e.Description ?? string.Empty,
                    EndAt = e.EndAt,
                    Id = e.Id,
                    StartAt = e.StartAt,
                    Title = e.Title,
                    TotalSeats = e.TotalSeats
                })
                .ToList(),
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages
        };
    }

    /// <summary>
    /// Получение события по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    /// <response code="200">Событие получено</response>
    /// <response code="404">Неверные данные события</response>
    [HttpGet("{id:Guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResultDto<EventInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status404NotFound)]
    public async Task<ApiResultDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var @event = await eventService.GetByIdAsync(@id, ct);

        return new ApiResultDto<EventInfoDto>
        {
            Data = new EventInfoDto
            {
                AvailableSeats = @event.AvailableSeats,
                Description = @event.Description ?? string.Empty,
                EndAt = @event.EndAt,
                Id = @event.Id,
                StartAt = @event.StartAt,
                Title = @event.Title,
                TotalSeats = @event.TotalSeats
            },
            Success = true,
            StatusCode = HttpStatusCode.OK
        };
    }


    /// <summary>
    /// Создание нового события
    /// </summary>
    /// <param name="eventDto">Данные события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    /// <response code="201">Событие создано</response>
    /// <response code="400">Неверные данные события</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResultDto<Event>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResultDto<EventInfoDto>>> Post([FromBody] CreateEventDto eventDto, CancellationToken ct)
    {
        var @event = await eventService.CreateAsync(eventDto.Title, eventDto.StartAt, eventDto.EndAt, eventDto.TotalSeats, eventDto.Description, ct);

        return CreatedAtAction("GetById",
            new { id = @event.Id },
            new ApiResultDto<EventInfoDto>
            {
                Data = new EventInfoDto
                {
                    AvailableSeats = @event.AvailableSeats,
                    Description = @event.Description ?? string.Empty,
                    EndAt = @event.EndAt,
                    Id = @event.Id,
                    StartAt = @event.StartAt,
                    Title = @event.Title,
                    TotalSeats = @event.TotalSeats
                },
                Success = true,
                StatusCode = HttpStatusCode.Created
            });
    }

    /// <summary>
    /// Обновление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="eventDto">Данные события</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="204">Успешное обновление</response>
    /// <response code="400">Неверные данные события</response>
    /// <response code="404">Событие не найдено</response>
    [HttpPut("{id:Guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> PutAsync(Guid id, [FromBody] UpdateEventDto eventDto, CancellationToken ct)
    {
        await eventService.UpdateAsync(id, eventDto.Title, eventDto.StartAt, eventDto.EndAt, eventDto.TotalSeats, eventDto.Description, ct);

        return NoContent();
    }

    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>NoContentResult</returns>
    /// <response code="204">Событие удалено</response>
    /// <response code="404">Событие не найдено</response>
    [HttpDelete("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        await eventService.RemoveAsync(id, ct);

        return NoContent();
    }

    /// <summary>
    /// Создание брони
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Бронь</returns>
    /// <response code="202">Принято в обработку</response>
    /// <response code="400">Не корректный запрос</response>
    /// <response code="404">Событие не найдено</response>
    [HttpPost("{id:Guid}/book")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResultDto<BookingOutDto>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResultDto>> CreateBookingAsync(Guid id, CancellationToken ct = default)
    {
        var booking = await bookingService.CreateBookingAsync(id, ct);

        return AcceptedAtAction(
            actionName: "Get",
            controllerName: "Bookings",
            routeValues: new { bookingId = booking.Id },
            value: new ApiResultDto<BookingOutDto>
                {
                    Data = new BookingOutDto(booking.Id, booking.EventId, booking.Status.ToString()),
                    StatusCode = HttpStatusCode.Accepted,
                    Success = true
                }
        );
    }
}
