using EventManagmentApi.Application.Exceptions;
using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Models;
using EventManagmentApi.Presentation.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventManagmentApi.Presentation.Controllers;

/// <summary>
/// Контроллер обработки событий
/// </summary>
/// <param name="eventService"></param>
[ApiController]
[Route("api/events")]
public class EventsController(IEventService eventService) : ControllerBase
{
    /// <summary>
    /// Получение всего списка событий
    /// </summary>
    /// <returns>Весь список событий</returns>
    /// <response code="200">Весь список событий</response>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResult<IReadOnlyList<Event>>), StatusCodes.Status200OK)]
    public ApiResult<IReadOnlyList<Event>> GetAll() =>
        new ApiResult<IReadOnlyList<Event>>
        {
            Data = eventService.GetAll(),
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
    public ApiResultBase Get(int id)
    {
        try
        {
            return new ApiResult<Event>
            {
                Data = eventService.Get(id),
                Success = true,
                StatusCode= HttpStatusCode.OK
            };
        }
        catch (NotFoundException e)
        {
            return new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = e.Message
            };
        }
    }


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
    public ApiResultBase Post([FromBody] EventDto eventDto)
    {
        try
        {
            var @event = eventService.Create(eventDto.Title, eventDto.StartAt.Value, eventDto.EndAt.Value, eventDto.Description);

            return new ApiResult<Event>
            {
                Data = @event,
                Success = true,
                StatusCode = HttpStatusCode.Created
            };
        }
        catch (Exception e)
        {
            return new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                Message = e.Message
            };
        }
    }

    /// <summary>
    /// Обновление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="eventDto">Данные события</param>
    /// <response code="204"></response>
    /// <response code="400">Неверные данные события</response>
    /// <response code="404">Событие не найдено</response>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public ApiResult Put(int id, [FromBody] EventDto eventDto)
    {
        try
        {
            eventService.Update(id, eventDto.Title, eventDto.StartAt.Value, eventDto.EndAt.Value, eventDto.Description);

            return new ApiResult
            {
                Success = true,
                StatusCode = HttpStatusCode.NoContent
            };
        }
        catch (NotFoundException e)
        {
            return new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = e.Message
            };
        }
        catch (Exception e)
        {
            return new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                Message = e.Message
            };
        }
    }

    /// <summary>
    /// Удаление события
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <returns></returns>
    /// <response code="204">Событие удалено</response>
    /// <response code="404">Событие не найдено</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public ApiResult Delete(int id)
    {
        try
        {
            eventService.Remove(id);

            return new ApiResult
            {
                Success = true,
                StatusCode = HttpStatusCode.NoContent
            };
        }
        catch (NotFoundException e)
        {
            return new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = e.Message
            };
        }
    }
}
