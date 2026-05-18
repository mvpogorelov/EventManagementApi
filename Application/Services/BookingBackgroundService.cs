using EventManagmentApi.Application.Enums;
using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Application.Models;

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
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

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
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

                    var pendingBookings = await bookingService.GetByStatusAsync(BookingStatus.Pending, ct);
                    var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, bookingService, eventService, ct));

                    await Task.WhenAll(tasks);
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

        private async Task ProcessBookingAsync(Booking booking, IBookingService bookingService, IEventService eventService, CancellationToken ct)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), ct);

            await _processingSemaphore.WaitAsync(ct);
            try
            {
                var @event = eventService.Get(booking.EventId);

                if (@event is null)
                {
                    await bookingService.UpdateStatusAsync(booking.Id, BookingStatus.Rejected, ct);

                    logger.LogWarning($"Бронь {booking.Id} отклонена, отсутствует событие {booking.EventId}");
                }
                else
                {
                    await bookingService.UpdateStatusAsync(booking.Id, BookingStatus.Confirmed, ct);
                }

            }
            catch (Exception e)
            {
                await bookingService.UpdateStatusAsync(booking.Id, BookingStatus.Rejected, ct);
                eventService.ReleaseSeats(booking.EventId);

                logger.LogError($"Неожиданная ошибка при обработке брони {booking.Id}: {e}");
            }
            finally
            { 
                _processingSemaphore.Release();
            }
        }
    }
}
