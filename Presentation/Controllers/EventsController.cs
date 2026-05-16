using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Application.Models;
using EventManagmentApi.Presentation.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventManagmentApi.Presentation.Controllers;

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
    [ProducesResponseType(typeof(PaginatedResultDto<IReadOnlyList<Event>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status400BadRequest)]
    public PaginatedResultDto<IReadOnlyList<Event>> GetAll(
        [FromQuery] string? title,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = eventService.GetAll(title, from, to, page, pageSize);

        return new PaginatedResultDto<IReadOnlyList<Event>>
        {
            Data = result.Items,
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
    /// <returns>Событие</returns>
    /// <response code="200">Событие получено</response>
    /// <response code="404">Неверные данные события</response>
    [HttpGet("{id:Guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResultDto<Event>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status404NotFound)]
    public ApiResultDto Get(Guid id) =>
        new ApiResultDto<Event>
        {
            Data = eventService.Get(id),
            Success = true,
            StatusCode = HttpStatusCode.OK
        };


    /// <summary>
    /// Создание нового события
    /// </summary>
    /// <param name="eventDto">Данные события</param>
    /// <returns>Событие</returns>
    /// <response code="201">Событие создано</response>
    /// <response code="400">Неверные данные события</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResultDto<Event>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResultDto> Post([FromBody] EventDto eventDto)
    {
        var @event = eventService.Create(eventDto.Title, eventDto.StartAt, eventDto.EndAt, eventDto.TotalSeats, eventDto.Description);

        return CreatedAtAction(nameof(Get),
            new { id = @event.Id },
            new ApiResultDto<Event>
            {
                Data = @event,
                Success = true,
                StatusCode = HttpStatusCode.Created
            });
    }

    /// <summary>
    /// Обновление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="eventDto">Данные события</param>
    /// <response code="204">Успешное обновление</response>
    /// <response code="400">Неверные данные события</response>
    /// <response code="404">Событие не найдено</response>
    [HttpPut("{id:Guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status404NotFound)]
    public NoContentResult Put(Guid id, [FromBody] EventDto eventDto)
    {
        eventService.Update(id, eventDto.Title, eventDto.StartAt, eventDto.EndAt, eventDto.TotalSeats, eventDto.Description);

        return NoContent();
    }

    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <returns></returns>
    /// <response code="204">Событие удалено</response>
    /// <response code="404">Событие не найдено</response>
    [HttpDelete("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status404NotFound)]
    public NoContentResult Delete(Guid id)
    {
        eventService.Remove(id);

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
