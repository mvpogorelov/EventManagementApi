using EventManagmentApi.Application.Enums;
using EventManagmentApi.Application.Exceptions;
using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Application.Models;
using EventManagmentApi.Application.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace EventManagmentApi.Tests.Application.Services;

public class BookingServiceTests
{
    private readonly BookingService _bookingService;
    private readonly IEventService _eventService;

    public BookingServiceTests()
    {
        _eventService = Substitute.For<IEventService>();
        _bookingService = new BookingService(_eventService);
    }

    [Fact(DisplayName = "Для существующего события должна создаваться бронь со статусом Pending")]
    public async Task Create_WhenEventExists_ShouldCreateBookingWithPendingStatus()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new Event {
            Id = eventId,
            Title = "Title",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(10),
            TotalSeats = 1,
        };

        _eventService.Get(eventId).Returns(@event);

        // Act
        var booking = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        // Assert
        Assert.Equal(eventId, booking.EventId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }
    
    [Fact(DisplayName = "Создание нескольких броней для одного события — все создаются с уникальными Id")]
    public async Task Create_MultipleBookingForSingleEvent_ShouldCreateBookingWithUniqueId()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new Event {
            Id = eventId,
            Title = "Title",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(10),
            TotalSeats = 1,
        };

        _eventService.Get(eventId).Returns(@event);

        // Act
        var booking1 = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var booking2 = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var booking3 = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);

        // Assert
        Assert.NotEqual(booking1.Id, booking2.Id);
        Assert.NotEqual(booking1.Id, booking3.Id);
        Assert.NotEqual(booking2.Id, booking3.Id);
    }
    
    [Fact(DisplayName = "Получение брони по Id — возвращается корректная информация")]
    public async Task Create_GetBookingById_ShouldReturnCorrectData()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new Event {
            Id = eventId,
            Title = "Title",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(10),
            TotalSeats = 1,
        };

        _eventService.Get(eventId).Returns(@event);

        // Act
        var booking = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var getByIdBooking = await _bookingService.GetBookingByIdAsync(booking.Id, CancellationToken.None);

        // Assert
        Assert.Equal(booking, getByIdBooking);
    }
    
    [Fact(DisplayName = "Получение брони отражает изменение статуса")]
    public async Task Create_ChangeStatus_ShouldReflectChanges()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = new Event {
            Id = eventId,
            Title = "Title",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(10),
            TotalSeats = 1,
        };

        _eventService.Get(eventId).Returns(@event);

        // Act
        var bookingAfterCreate = await _bookingService.CreateBookingAsync(eventId, CancellationToken.None);
        var bookingAfterCreateStatus = bookingAfterCreate.Status;
        var bookingAfterCreateProcessedAt = bookingAfterCreate.ProcessedAt;

        await _bookingService.UpdateStatusAsync(bookingAfterCreate.Id, BookingStatus.Confirmed, CancellationToken.None);

        var bookingAfterChangeStatus = await _bookingService.GetBookingByIdAsync(bookingAfterCreate.Id, CancellationToken.None);

        // Assert
        Assert.Equal(BookingStatus.Pending, bookingAfterCreateStatus);
        Assert.Null(bookingAfterCreateProcessedAt);
        Assert.Equal(BookingStatus.Confirmed, bookingAfterChangeStatus.Status);
        Assert.NotNull(bookingAfterChangeStatus.ProcessedAt);
    }

    [Fact(DisplayName = "Для несуществующего события должна выбрасываться ошибка")]
    public async Task Create_WhenEventDoNotExists_ShouldRiseException()
    {
        // Arrange
        var eventId = Guid.NewGuid();

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

        _eventService.Get(eventId).Throws(new NotFoundException());

        // Act
        var ex = await Record.ExceptionAsync(async () => await _bookingService.CreateBookingAsync(eventId, CancellationToken.None));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<NotFoundException>(ex);
    }
}
