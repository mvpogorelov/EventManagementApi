using EventManagmentApi.Application.Enums;
using EventManagmentApi.Application.Exceptions;
using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Application.Models;
using EventManagmentApi.Application.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EventManagmentApi.Tests.Application.Services;

public class BookingServiceTests
{
    private readonly BookingService _bookingService;
    private readonly IEventService _eventService;

    public BookingServiceTests()
    {
        _eventService = Substitute.For<IEventService>();
        var logger = Substitute.For<ILogger<BookingService>>();

        _bookingService = new BookingService(_eventService, logger);
    }

    [Fact(DisplayName = "Для существующего события должна создаваться бронь со статусом Pending")]
    public async Task Create_WhenEventExists_ShouldCreateBookingWithPendingStatus()
    {
        // Arrange
        var @event = new Event("Title", DateTime.UtcNow, DateTime.UtcNow.AddDays(10), 1);

        _eventService.Get(@event.Id).Returns(@event);

        // Act
        var booking = await _bookingService.CreateBookingAsync(@event.Id, CancellationToken.None);

        // Assert
        Assert.Equal(@event.Id, booking.EventId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }
    
    [Fact(DisplayName = "Создание нескольких броней для одного события — все создаются с уникальными Id")]
    public async Task Create_MultipleBookingForSingleEvent_ShouldCreateBookingWithUniqueId()
    {
        // Arrange
        var @event = new Event("Title", DateTime.UtcNow, DateTime.UtcNow.AddDays(10), 1);

        _eventService.Get(@event.Id).Returns(@event);

        // Act
        var booking1 = await _bookingService.CreateBookingAsync(@event.Id, CancellationToken.None);
        var booking2 = await _bookingService.CreateBookingAsync(@event.Id, CancellationToken.None);
        var booking3 = await _bookingService.CreateBookingAsync(@event.Id, CancellationToken.None);

        // Assert
        Assert.NotEqual(booking1.Id, booking2.Id);
        Assert.NotEqual(booking1.Id, booking3.Id);
        Assert.NotEqual(booking2.Id, booking3.Id);
    }
    
    [Fact(DisplayName = "Получение брони по Id — возвращается корректная информация")]
    public async Task Create_GetBookingById_ShouldReturnCorrectData()
    {
        // Arrange
        var @event = new Event("Title", DateTime.UtcNow, DateTime.UtcNow.AddDays(10), 1);

        _eventService.Get(@event.Id).Returns(@event);

        // Act
        var booking = await _bookingService.CreateBookingAsync(@event.Id, CancellationToken.None);
        var getByIdBooking = await _bookingService.GetBookingByIdAsync(booking.Id, CancellationToken.None);

        // Assert
        Assert.Equal(booking, getByIdBooking);
    }
    
    [Theory(DisplayName = "Получение брони отражает изменение статуса")]
    [InlineData(BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Rejected)]
    public async Task Create_ChangeStatus_ShouldReflectChanges(BookingStatus status)
    {
        // Arrange
        var @event = new Event("Title", DateTime.UtcNow, DateTime.UtcNow.AddDays(10), 1);

        _eventService.Get(@event.Id).Returns(@event);

        // Act
        var bookingAfterCreate = await _bookingService.CreateBookingAsync(@event.Id, CancellationToken.None);
        var bookingAfterCreateStatus = bookingAfterCreate.Status;
        var bookingAfterCreateProcessedAt = bookingAfterCreate.ProcessedAt;

        await _bookingService.UpdateStatusAsync(bookingAfterCreate.Id, status, CancellationToken.None);

        var bookingAfterChangeStatus = await _bookingService.GetBookingByIdAsync(bookingAfterCreate.Id, CancellationToken.None);

        // Assert
        Assert.Equal(BookingStatus.Pending, bookingAfterCreateStatus);
        Assert.Null(bookingAfterCreateProcessedAt);
        Assert.Equal(status, bookingAfterChangeStatus.Status);
        Assert.NotNull(bookingAfterChangeStatus.ProcessedAt);
    }

    [Fact(DisplayName = "Для несуществующего события должна выбрасываться ошибка")]
    public async Task Create_WhenEventDoNotExists_ShouldRiseException()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        _eventService.TryReserveSeats(eventId).Throws(new NotFoundException());

        // Act
        var ex = await Record.ExceptionAsync(async () => await _bookingService.CreateBookingAsync(eventId, CancellationToken.None));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<NotFoundException>(ex);
    }

    [Fact(DisplayName = "Получение брони по несуществующему Id должно завершаться ошибкой")]
    public async Task GetById_WhenIdDoesNotExists_ShouldRiseException()
    {
        // Arrange
        var bookingId = Guid.NewGuid();

        // Act
        var ex = await Record.ExceptionAsync(async () => await _bookingService.GetBookingByIdAsync(bookingId, CancellationToken.None));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<NotFoundException>(ex);
    }

    [Fact(DisplayName = "Для удалённого события должна выбрасываться ошибка")]
    public async Task Create_WhenEventDeleted_ShouldRiseException()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        _eventService.TryReserveSeats(eventId).Throws(new NotFoundException());

        // Act
        var ex = await Record.ExceptionAsync(async () => await _bookingService.CreateBookingAsync(eventId, CancellationToken.None));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<NotFoundException>(ex);
    }
    
    [Fact(DisplayName = "20 параллельных запросов на 5 мест")]
    public async Task Create_When20BookingsOn5Seats_ShouldCorrectResult()
    {
        // Arrange
        var eventService = new EventService();
        var logger = Substitute.For<ILogger<BookingService>>();
        var bookingService = new BookingService(eventService, logger);
        var @event = eventService.Create("Title", DateTime.UtcNow, DateTime.UtcNow.AddDays(10), 5);
        var tasks = Enumerable.Range(1, 20).Select(i => bookingService.CreateBookingAsync(@event.Id, CancellationToken.None)).ToArray();

        // Act
        await Task.WhenAll(tasks).ContinueWith(_ => { });

        // Assert
        Assert.Equal(5, tasks.Where(t => t.Status == TaskStatus.RanToCompletion).Count()); // 5 броней создалось
        Assert.Equal(
            15,
            tasks
                .Where(t => t.IsFaulted && t.Exception != null)
                .SelectMany(t => t.Exception!.InnerExceptions)
                .Where(e => e is NoAvailableSeatsException)
                .Count()
        ); //15 упали в ошибку NoAvailableSeatsException
    }

    [Fact(DisplayName = "10 параллельных запросов на 10 мест")]
    public async Task Create_When10BookingsOn10Seats_ShouldCorrectResult()
    {
        // Arrange
        var eventService = new EventService();
        var logger = Substitute.For<ILogger<BookingService>>();
        var bookingService = new BookingService(eventService, logger);
        var @event = eventService.Create("Title", DateTime.UtcNow, DateTime.UtcNow.AddDays(10), 10);
        var tasks = Enumerable.Range(1, 10).Select(i => bookingService.CreateBookingAsync(@event.Id, CancellationToken.None)).ToArray();

        // Act
        await Task.WhenAll(tasks).ContinueWith(_ => { });

        // Assert
        Assert.Equal(10, tasks.Where(t => t.Status == TaskStatus.RanToCompletion).Count()); // 10 броней создалось

        var bookingIds = tasks.Select(t => t.Result.Id).ToArray();

        Assert.Equal(bookingIds.Length, new HashSet<Guid>(bookingIds).Count()); // имеют уникальные Id
    }
}
