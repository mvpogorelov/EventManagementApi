using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Models;
using EventManagmentApi.Presentation.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EventManagmentApi.Presentation.Controllers;

/// <summary>
/// Контроллер обработки событий
/// </summary>
/// <param name="eventService"></param>
[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    /// <summary>
    /// Получение всего списка событий
    /// </summary>
    /// <returns>Весь список событий</returns>
    /// <response code="200">Весь список событий</response>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ActionResult<IReadOnlyList<Event>>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<Event>> GetAll() => Ok(eventService.GetAll());

    /// <summary>
    /// Получение события по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <returns>Событие</returns>
    /// <response code="200">Событие получено</response>
    /// <response code="404">Неверные данные события</response>
    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ActionResult<Event>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public IActionResult Get(int id)
    {
        try
        {
            return Ok(eventService.Get(id));
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] EventDto eventDto)
    {
        try
        {
            var @event = eventService.Create(eventDto.Title, eventDto.StartAt.Value, eventDto.EndAt.Value, eventDto.Description);

            return CreatedAtAction(nameof(Get), new { id = @event.Id }, @event);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public IActionResult Put(int id, [FromBody] EventDto eventDto)
    {
        try
        {
            eventService.Update(id, eventDto.Title, eventDto.StartAt.Value, eventDto.EndAt.Value, eventDto.Description);

            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        try
        {
            eventService.Remove(id);

            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}
