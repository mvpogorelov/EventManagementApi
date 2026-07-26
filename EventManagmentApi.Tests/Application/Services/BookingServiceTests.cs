using EventManagement.Application.Abstractions.Persistence.Repositories;
using EventManagement.Application.Abstractions.Services;
using EventManagement.Application.Services;
using EventManagement.Domain.Common;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using EventManagement.Infrastructure.Persistence;
using EventManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventManagmentApi.Tests.Application.Services;

public class BookingServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;
    private readonly IEventService _eventService;
    private readonly IBookingService _bookingService;
    
    private Event testEvent;
    private Guid testUserId;
    private Guid testAdminId;

    public BookingServiceTests()
    {
        var serviceCollection = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();

        serviceCollection.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        serviceCollection.AddScoped<IEventRepository, EventRepository>();
        serviceCollection.AddScoped<IBookingRepository, BookingRepository>();
        serviceCollection.AddScoped<IEventService, EventService>();
        serviceCollection.AddScoped<IBookingService, BookingService>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _serviceScope = _serviceProvider.CreateScope();
        _eventService = _serviceScope.ServiceProvider.GetRequiredService<IEventService>();
        _bookingService = _serviceScope.ServiceProvider.GetRequiredService<IBookingService>();
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
        _serviceProvider.Dispose();
    }

    [Fact(DisplayName = "Для существующего события должна создаваться бронь со статусом Pending")]
    public async Task Create_WhenEventExists_ShouldCreateBookingWithPendingStatus()
    {
        // Arrange
        await SetTestData();

        // Act
        var booking = await _bookingService.CreateBookingAsync(testEvent.Id, testUserId, CancellationToken.None);

        // Assert
        Assert.Equal(testEvent.Id, booking.EventId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }
    
    [Fact(DisplayName = "Создание нескольких броней для одного события — все создаются с уникальными Id")]
    public async Task Create_MultipleBookingForSingleEvent_ShouldCreateBookingWithUniqueId()
    {
        // Arrange
        await SetTestData();

        // Act
        var booking1 = await _bookingService.CreateBookingAsync(testEvent.Id, testUserId, CancellationToken.None);
        var booking2 = await _bookingService.CreateBookingAsync(testEvent.Id, testUserId, CancellationToken.None);
        var booking3 = await _bookingService.CreateBookingAsync(testEvent.Id, testUserId, CancellationToken.None);

        // Assert
        Assert.NotEqual(booking1.Id, booking2.Id);
        Assert.NotEqual(booking1.Id, booking3.Id);
        Assert.NotEqual(booking2.Id, booking3.Id);
    }
    
    [Fact(DisplayName = "Получение своей брони (или Admin) по Id — возвращается корректная информация")]
    public async Task Create_OwnOrAdminGetBookingById_ShouldReturnCorrectData()
    {
        // Arrange
        await SetTestData();

        // Act
        var bookingUser = await _bookingService.CreateBookingAsync(testEvent.Id, testUserId, CancellationToken.None);
        var getByIdBookingUserUser = await _bookingService.GetBookingByIdAsync(bookingUser.Id, testUserId, UserRole.User, CancellationToken.None);
        var getByIdBookingUserAdmin = await _bookingService.GetBookingByIdAsync(bookingUser.Id, testAdminId, UserRole.Admin, CancellationToken.None);

        // Assert
        Assert.Equal(bookingUser, getByIdBookingUserUser);
        Assert.Equal(bookingUser, getByIdBookingUserAdmin);
    }
    
    [Fact(DisplayName = "Получение не своей брони по Id — возвращается корректная информация")]
    public async Task Create_NotOwnGetBookingById_ShouldReturnCorrectData()
    {
        // Arrange
        await SetTestData();

        // Act
        var bookingAdmin = await _bookingService.CreateBookingAsync(testEvent.Id, testAdminId, CancellationToken.None);
        var ex = await Record.ExceptionAsync(async () => await _bookingService.GetBookingByIdAsync(bookingAdmin.Id, testUserId, UserRole.User, CancellationToken.None));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<OperationNotAllowedException>(ex);
    }
    
    [Fact(DisplayName = "Для несуществующего события должна выбрасываться ошибка")]
    public async Task Create_WhenEventDoNotExists_ShouldRiseException()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var ex = await Record.ExceptionAsync(async () => await _bookingService.CreateBookingAsync(eventId, testUserId, CancellationToken.None));

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
        var ex = await Record.ExceptionAsync(async () =>
            await _bookingService.GetBookingByIdAsync(
                bookingId,
                testAdminId,
                UserRole.Admin,
                CancellationToken.None));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<NotFoundException>(ex);
    }

    [Fact(DisplayName = "Для удалённого события должна выбрасываться ошибка")]
    public async Task Create_WhenEventDeleted_ShouldRiseException()
    {
        // Arrange
        await SetTestData();
        await _eventService.RemoveAsync(testEvent.Id, CancellationToken.None);

        // Act
        var ex = await Record.ExceptionAsync(async () => await _bookingService.CreateBookingAsync(testEvent.Id, testUserId, CancellationToken.None));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<NotFoundException>(ex);
    }
    
    [Fact(DisplayName = "20 параллельных запросов на 5 мест")]
    public async Task Create_When20BookingsOn5Seats_ShouldCorrectResult()
    {
        // Arrange
        await SetTestData(totalSeats: 5);
        var tasks = Enumerable.Range(1, 20).Select(i => _bookingService.CreateBookingAsync(testEvent.Id, testUserId, CancellationToken.None)).ToArray();

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
        await SetTestData(totalSeats: 10);
        var tasks = Enumerable.Range(1, 10).Select(i => _bookingService.CreateBookingAsync(testEvent.Id, testUserId, CancellationToken.None)).ToArray();

        // Act
        await Task.WhenAll(tasks).ContinueWith(_ => { });

        // Assert
        Assert.Equal(10, tasks.Where(t => t.Status == TaskStatus.RanToCompletion).Count()); // 10 броней создалось

        var bookingIds = tasks.Select(t => t.Result.Id).ToArray();

        Assert.Equal(bookingIds.Length, new HashSet<Guid>(bookingIds).Count()); // имеют уникальные Id
    }

    private async Task SetTestData(int totalSeats = 100, CancellationToken ct = default)
    {
        await _eventService.RemoveAllAsync(ct);
        testEvent = await _eventService.CreateAsync("Title", DateTime.UtcNow, DateTime.UtcNow.AddDays(10), totalSeats, "Desctiption", ct);
        testUserId = Guid.NewGuid();
        testAdminId = Guid.NewGuid();
    }
}
