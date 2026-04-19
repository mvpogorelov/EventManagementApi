using EventManagmentApi.Application.Exceptions;
using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Models;
using EventManagmentApi.Presentation.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static System.Net.WebRequestMethods;

namespace EventManagmentApi.Presentation.Controllers;

/// <summary>
/// Контроллер обработки событий
/// </summary>
/// <param name="eventService"></param>
[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    /// <summary>
    /// Получение всего списка событий
    /// </summary>
    /// <param name="title">Фильтр по названию</param>
    /// <param name="from">С даты</param>
    /// <param name="to">По дату</param>
    /// <returns>Весь список событий</returns>
    /// <response code="200">Весь список событий</response>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<Event>>), StatusCodes.Status200OK)]
    public ApiResult<IReadOnlyList<Event>> GetAll(
        [FromQuery] string? title,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to) =>
            new ApiResult<IReadOnlyList<Event>>
            {
                Data = eventService.GetAll(title, from, to),
                Success = true,
                StatusCode = HttpStatusCode.OK
            };

    /// <summary>
    /// Получение события по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <returns>Событие</returns>
    /// <response code="200">Событие получено</response>
    /// <response code="404">Неверные данные события</response>
    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResult<Event>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public ApiResult Get(int id) =>
        new ApiResult<Event>
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
    [ProducesResponseType(typeof(ApiResult<Event>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    public ActionResult<ApiResult> Post([FromBody] EventDto eventDto)
    {
        var @event = eventService.Create(eventDto.Title, eventDto.StartAt, eventDto.EndAt, eventDto.Description);

        return CreatedAtAction(nameof(Get),
            new { id = @event.Id },
            new ApiResult<Event>
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
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public NoContentResult Put(int id, [FromBody] EventDto eventDto)
    {
        eventService.Update(id, eventDto.Title, eventDto.StartAt, eventDto.EndAt, eventDto.Description);

        return NoContent();
    }

    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <returns></returns>
    /// <response code="204">Событие удалено</response>
    /// <response code="404">Событие не найдено</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public NoContentResult Delete(int id)
    {
        eventService.Remove(id);

        return NoContent();
    }
}
