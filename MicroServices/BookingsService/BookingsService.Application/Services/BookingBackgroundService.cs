using BookingsService.Application.Abstractions.Persistence.Repositories;
using BookingsService.Application.Abstractions.Services;
using BookingsService.Domain.Common;
using BookingsService.Domain.Entities;
using EventManagement.Contracts.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingsService.Application.Services;

public class BookingBackgroundService(
    ILogger<BookingBackgroundService> logger,
    IServiceScopeFactory scopeFactory
    //,
    //IKafkaProducerService kafkaProducer
    )
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
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                var pendingBookings = await bookingRepository.GetByStatusAsync(BookingStatus.Pending, ct);
                var tasks = pendingBookings.Select(booking => ProcessBookingAsync(bookingRepository, booking, ct));

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
    /// <param name="bookingRepository"></param>
    /// <param name="booking">Бронь</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task ProcessBookingAsync(
        IBookingRepository bookingRepository,
        Booking booking,
        CancellationToken ct)
    {
        await _processingSemaphore.WaitAsync(ct);

        try
        {
            booking.Processing();
            await bookingRepository.UpdateAsync(booking, ct);

            //await kafkaProducer.PublishAsync("bookings",
            //    booking.Id.ToString(),
            //    new BookingProcessing
            //    {
            //        BookingId = booking.Id,
            //        EventId = booking.EventId,
            //        UserId = booking.UserId
            //    });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            logger.LogInformation("Обработка прервана");

            throw;
        }
        catch (Exception e)
        {
            logger.LogError($"Неожиданная ошибка при обработке брони {booking.Id}: {e}");

            throw;
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }
}
