using EventManagement.Application.Abstractions.Services;
using EventManagement.Presentation.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventManagement.Presentation.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bookingService"></param>
    [Route("[controller]")]
    [ApiController]
    public class BookingsController(IBookingService bookingService) : ControllerBase
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
            var booking = await bookingService.GetBookingByIdAsync(bookingId, ct);

            return new ApiResultDto<BookingOutDto>
            {
                Data = new BookingOutDto(booking.Id, booking.EventId, booking.Status.ToString()),
                Success = true,
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}
