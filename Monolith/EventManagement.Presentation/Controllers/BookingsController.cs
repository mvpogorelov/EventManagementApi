using EventManagement.Application.Abstractions.Services;
using EventManagement.Presentation.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventManagement.Presentation.Controllers;

/// <summary>
/// 
/// </summary>
/// <param name="bookingService"></param>
[Route("[controller]")]
[ApiController]
public class BookingsController(IBookingService bookingService) : BaseController
{
    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Событие</returns>
    /// <response code="200">Событие получено</response>
    /// <response code="404">Неверные данные события</response>
    [HttpGet("{bookingId:Guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResultDto<BookingOutDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status404NotFound)]
    public async Task<ApiResultDto<BookingOutDto>> Get(Guid bookingId, CancellationToken ct)
    {
        var booking = await bookingService.GetBookingByIdAsync(bookingId, CurrentUserId, CurrentUserRole, ct);

        return new ApiResultDto<BookingOutDto>
        {
            Data = new BookingOutDto(booking.Id, booking.EventId, booking.Status.ToString()),
            Success = true,
            StatusCode = HttpStatusCode.OK
        };
    }

    /// <summary>
    /// Отмена брони
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>NoContentResult</returns>
    /// <response code="204">Бронь отменена</response>
    /// <response code="404">Бронь не найдена</response>
    [HttpPost("{bookingId:Guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> CancelAsync(Guid bookingId, CancellationToken ct)
    {
        await bookingService.CancelAsync(bookingId, CurrentUserId, CurrentUserRole, ct);

        return NoContent();
    }

    /// <summary>
    /// Удаление брони
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>NoContentResult</returns>
    /// <response code="204">Бронь удалена</response>
    /// <response code="404">Бронь не найдена</response>
    [HttpDelete("{bookingId:Guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> DeleteAsync(Guid bookingId, CancellationToken ct)
    {
        await bookingService.RemoveAsync(bookingId, CurrentUserId, CurrentUserRole, ct);

        return NoContent();
    }
}
