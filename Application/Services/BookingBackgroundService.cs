using EventManagmentApi.Application.Enums;
using EventManagmentApi.Application.Interfaces;

namespace EventManagmentApi.Application.Services
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="scopeFactory"></param>
    public class BookingBackgroundService(
        ILogger<BookingBackgroundService> logger,
        IServiceScopeFactory scopeFactory)
            : BackgroundService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            logger.LogInformation("BookingBackgroundService запущен");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    var pendingBookings = await bookingService.GetByStatusAsync(BookingStatus.Pending, ct);

                    foreach ( var pendingBooking in pendingBookings)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2), ct);
                        await bookingService.UpdateStatusAsync(pendingBooking.Id, BookingStatus.Confirmed, ct);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Ошибка при обработке брони");
                }
            }

            logger.LogInformation("BookingBackgroundService остановлен");
        }
    }
}
