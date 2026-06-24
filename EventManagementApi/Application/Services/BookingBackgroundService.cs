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
        private const int PollingInterval = 10000;
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
                    var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                    var pendingBookings = await bookingService.GetByStatusAsync(BookingStatus.Pending, ct);
                    var tasks = pendingBookings.Select(booking => ProcessBookingAsync(eventRepository, bookingService, booking, ct));

                    await Task.WhenAll(tasks);
                    await Task.Delay(PollingInterval, ct);
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

        /// <summary>
        /// Обработка брони
        /// </summary>
        /// <param name="eventRepository"></param>
        /// <param name="bookingService"></param>
        /// <param name="booking">Бронь</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task ProcessBookingAsync(IEventRepository eventRepository, IBookingService bookingService, Booking booking, CancellationToken ct)
        {
            Event? @event = null;

            await _processingSemaphore.WaitAsync(ct);
            try
            {
                @event = await eventRepository.GetByIdAsync(booking.EventId, ct);

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
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                logger.LogInformation("Обработка прервана");

                throw;
            }
            catch (Exception e)
            {
                await bookingService.UpdateStatusAsync(booking.Id, BookingStatus.Rejected, ct);

                if (@event is not null)
                {
                    @event.ReleaseSeats();
                    await eventRepository.UpdateAsync(@event, ct);
                }

                logger.LogError($"Неожиданная ошибка при обработке брони {booking.Id}: {e}");

                throw;
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }
    }
}
