using EventManagmentApi.Application.Exceptions;
using EventManagmentApi.Application.Interfaces;
using EventManagmentApi.Application.Models;
using EventManagmentApi.Application.Repositories;
using EventManagmentApi.Application.Services;
using EventManagmentApi.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace EventManagmentApi.Tests.Application.Services;

public class EventServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;
    private readonly IEventService _eventService;

    private Event testEvent1, testEvent2, testEvent3, testEvent4;

    public EventServiceTests()
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
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
        _serviceProvider.Dispose();
    }

    private async Task SetTestData(CancellationToken ct = default)
    {
        await _eventService.RemoveAllAsync();

        testEvent1 = await _eventService.CreateAsync("Aa", new DateTime(2026, 4, 1), new DateTime(2026, 4, 10), 2, "AAaa", ct);
        testEvent2 = await _eventService.CreateAsync("Bb", new DateTime(2026, 3, 1), new DateTime(2026, 3, 10), 1, "BBbb", ct);
        testEvent3 = await _eventService.CreateAsync("Cc", new DateTime(2026, 2, 1), new DateTime(2026, 2, 10), 1, "CCcc", ct);
        testEvent4 = await _eventService.CreateAsync("Ccc", new DateTime(2026, 1, 1), new DateTime(2026, 1, 10), 1, "CCCccc", ct);
    }

    [Theory(DisplayName = "Создание: Если переданы неверные параметры, то должно выбрасываться исключение")]
    [MemberData(nameof(WrongEventParams))]
    public async Task Create_WhenParamsAreWrong_ShouldThrowException(string title, DateTime? startAt, DateTime? endAt, int totalSeats)
    {
        // Act
        var ex = await Record.ExceptionAsync(async () => await _eventService.CreateAsync(title, startAt, endAt, totalSeats, string.Empty, CancellationToken.None));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<ValidationException>(ex);
    }

    [Fact(DisplayName = "Создание: Если параметры верны, то должно создаться событие")]
    public async Task Create_WhenParamsAreCorrect_ShouldCreateEvent()
    {
        // Arrange
        var title = "Title";
        var startAt = new DateTime(2026, 4, 1);
        var endAt = new DateTime(2026, 4, 10);
        var totalSeats = 1;
        var description = "Description";

        // Act
        var @event = await _eventService.CreateAsync(title, startAt, endAt, totalSeats, description, CancellationToken.None);

        // Assert
        Assert.NotNull(@event);
        Assert.Equal(title, @event.Title);
        Assert.Equal(startAt, @event.StartAt);
        Assert.Equal(endAt, @event.EndAt);
        Assert.Equal(description, @event.Description);
    }

    [Theory(DisplayName = "Получение списка событий: Если переданы неверные параметры, то должно выбрасываться исключение")]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public async Task GetAll_WhenParamsAreWrong_ShouldThrowException(int page, int pageSize)
    {
        // Act
        var ex = await Record.ExceptionAsync(async () => await _eventService.GetAllAsync(null, null, null, page, pageSize));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<ArgumentOutOfRangeException>(ex);
    }

    [Fact(DisplayName = "Получение списка событий: Если фильтры не заданы, то должны вернуться все события")]
    public async Task GetAll_WhenFiltersAreNotDefined_ShouldReturnAllEvents()
    {
        // Arrange
        await SetTestData();

        // Act
        var events = await _eventService.GetAllAsync();

        // Assert
        Assert.Equal(4, events.Items.Count);
        Assert.Equal(1, events.Page);
        Assert.Equal(10, events.PageSize);
        Assert.Equal(4, events.TotalItems);
        Assert.Equal(1, events.TotalPages);
    }
    
    [Theory(DisplayName = "Получение списка событий: Если заданы параметры пагинации, то должны вернуться соответствующие данные")]
    [InlineData(1, 2, 2, 4, 2)]
    [InlineData(2, 1, 1, 4, 4)]
    [InlineData(1, 4, 4, 4, 1)]
    public async Task GetAll_WhenPaginationParamsAreDefined_ShouldReturnCorrectData(int page, int pageSize, int expectedItemsCount, int expectedTotalItems, int expectedTotalPages)
    {
        // Arrange
        await SetTestData();

        // Act
        var events = await _eventService.GetAllAsync(page: page, pageSize: pageSize);

        // Assert
        Assert.Equal(expectedItemsCount, events.Items.Count);
        Assert.Equal(page, events.Page);
        Assert.Equal(pageSize, events.PageSize);
        Assert.Equal(expectedTotalItems, events.TotalItems);
        Assert.Equal(expectedTotalPages, events.TotalPages);
    }

    [Theory(DisplayName = "Получение списка событий: Если задан фильтр по названию, то должны вернуться соотвествующие события")]
    [InlineData("AA")]
    [InlineData("aa")]
    [InlineData("BB")]
    [InlineData("bb")]
    public async Task GetAll_WhenTitleIsDefined_ShouldReturnCorrectEvents(string title)
    {
        // Arrange
        await SetTestData();

        // Act
        var events = await _eventService.GetAllAsync(title);

        // Assert
        Assert.Single(events.Items);
    }

    [Fact(DisplayName = "Получение списка событий: Если задан фильтр по дате начала, то должны вернуться соотвествующие события")]
    public async Task GetAll_WhenStartAtIsDefined_ShouldReturnCorrectEvents()
    {
        // Arrange
        await SetTestData();
        var startAt = new DateTime(2026, 3, 1);

        // Act
        var events = await _eventService.GetAllAsync(from: startAt);

        // Assert
        Assert.Equal(2, events.Items.Count);
    }
    
    [Fact(DisplayName = "Получение списка событий: Если задан фильтр по дате окончания, то должны вернуться соотвествующие события")]
    public async Task GetAll_WhenEndAtIsDefined_ShouldReturnCorrectEvents()
    {
        // Arrange
        await SetTestData();
        var endAt = new DateTime(2026, 3, 10);

        // Act
        var events = await _eventService.GetAllAsync(to: endAt);

        // Assert
        Assert.Equal(3, events.Items.Count);
    }

    [Fact(DisplayName = "Получение списка событий: Если задано несколько фильтров, то должны вернуться соотвествующие события")]
    public async Task GetAll_WhenFiltersAreDefined_ShouldReturnCorrectEvents()
    {
        // Arrange
        await SetTestData();

        var title = "cc";
        var startAt = new DateTime(2026, 1, 1);
        var endAt = new DateTime(2026, 3, 1);

        // Act
        var events = await _eventService.GetAllAsync(title, startAt, endAt);

        // Assert
        Assert.Equal(2, events.Items.Count);
    }

    [Theory(DisplayName = "Получение события по id: Если передан несущестующий id, то должен вернуться null")]
    [InlineData("3f2504e0-4f89-11d3-9a0c-0305e82c3301")]
    [InlineData("b0d4ce5d-2757-4699-948c-cfa72ba94f86")]
    public async Task Get_WhenIdIsIncorrect_ShouldThrowException(Guid id)
    {
        // Act
        var @event = await _eventService.GetByIdAsync(id, CancellationToken.None);

        // Assert
        Assert.Null(@event);
    }
    
    [Fact(DisplayName = "Получение события по id: Если передан сущестующий id, то должно вернуться событие")]
    public async Task Get_WhenIdIsСorrect_ShouldReturnEvent()
    {
        // Arrange
        await SetTestData();

        // Act
        var @event = await _eventService.GetByIdAsync(testEvent1.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(@event);
        Assert.Equal("Aa", @event.Title);
    }

    [Theory(DisplayName = "Обновление: Если переданы неверные параметры, то должно выбрасываться исключение")]
    [MemberData(nameof(WrongEventParams))]
    public async Task Update_WhenParamsAreWrong_ShouldThrowException(string title, DateTime? startAt, DateTime? endAt, int totalSeats)
    {
        // Arrange
        await SetTestData();

        // Act
        var ex = await Record.ExceptionAsync(async () => await _eventService.UpdateAsync(testEvent1.Id, title, startAt, endAt, totalSeats, string.Empty));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<ValidationException>(ex);
    }
    
    [Theory(DisplayName = "Обновление: Если переданы несуществующий id, то должно выбрасываться исключение")]
    [InlineData("b0d4ce5d-2757-4699-948c-cfa72ba94f86")]
    [InlineData("3f2504e0-4f89-11d3-9a0c-0305e82c3301")]
    public async Task Update_WhenIdIsMissing_ShouldThrowException(Guid id)
    {
        // Arrange
        var title = "Title";
        var startAt = new DateTime(2026, 1, 1);
        var endAt = new DateTime(2026, 3, 1);
        var totalSeats = 1;

        // Act
        var ex = await Record.ExceptionAsync(async () => await _eventService.UpdateAsync(id, title, startAt, endAt, totalSeats));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<NotFoundException>(ex);
    }

    [Fact(DisplayName = "Обновление: Если переданы корректные данные, то должно обновится событие")]
    public async Task Update_WhenParamsAreСorrect_ShouldUpdateEvent()
    {
        // Arrange
        await SetTestData();

        // Arrange
        var id = testEvent1.Id;
        var title = "Title";
        var startAt = new DateTime(2026, 1, 1);
        var endAt = new DateTime(2026, 3, 1);
        var totalSeats = 1;

        // Act
        var ex = await Record.ExceptionAsync(async () => await _eventService.UpdateAsync(id, title, startAt, endAt, totalSeats));

        // Assert
        Assert.Null(ex);
        Assert.Equal(title, testEvent1.Title);
        Assert.Equal(startAt, testEvent1.StartAt);
        Assert.Equal(endAt, testEvent1.EndAt);
        Assert.Equal(totalSeats, testEvent1.TotalSeats);
    }

    [Theory(DisplayName = "Удаление: Если переданы несуществующий id, то должно выбрасываться исключение")]
    [InlineData("b0d4ce5d-2757-4699-948c-cfa72ba94f86")]
    [InlineData("3f2504e0-4f89-11d3-9a0c-0305e82c3301")]
    public async Task Remove_WhenIdIsMissing_ShouldThrowException(Guid id)
    {
        // Act
        var ex = await Record.ExceptionAsync(async () => await _eventService.RemoveAsync(id));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<NotFoundException>(ex);
    }

    [Fact(DisplayName = "Удаление: Если переданы корректные данные, то должно удалиться событие")]
    public async Task Remove_WhenParamsAreСorrect_ShouldRemoveEvent()
    {
        // Arrange
        await SetTestData();

        // Act
        var ex = await Record.ExceptionAsync(async () => await _eventService.RemoveAsync(testEvent1.Id));

        // Assert
        Assert.Null(ex);
        // ToDo: добавить проверки, когда перейдём на другой источник данных
    }


    [Fact(DisplayName = "Попытка резервирования мест уменьшает AvailableSeats на 1")]
    public async Task ReserveSeats_WhenReserve_ShouldReduceAvailableSeats()
    {
        // Arrange
        await SetTestData();

        // Arrange
        var id = testEvent1.Id;
        var @event = await _eventService.GetByIdAsync(id);
        var avilableSeats = @event?.AvailableSeats;

        // Act
        var res = @event?.TryReserveSeats();

        // Assert
        Assert.True(res);
        Assert.Equal(avilableSeats - 1, @event?.AvailableSeats);
    }
    
    
    [Fact(DisplayName = "При попытке резервирования до лимита, должно возвращаться false")]
    public async Task ReserveSeats_WhenReserveToLimit_ShouldReturnFalse()
    {
        // Arrange
        await SetTestData();

        // Arrange
        var id = testEvent1.Id;
        var @event = await _eventService.GetByIdAsync(id);
        bool res;

        // Act
        do
        {
            res = @event.TryReserveSeats();
        }
        while (res);

        // Assert
        Assert.False(res);
    }
    
    [Fact(DisplayName = "При попытке резервирования при отсутствии мест должно выбрасываться исключение NoAvailableSeatsException")]
    public async Task ReserveSeats_WhenNoAvailableSeats_ShouldReturnFalse()
    {
        // Arrange
        await SetTestData();

        // Arrange
        var id = testEvent1.Id;
        var @event = await _eventService.GetByIdAsync(id);

        // Act
        var res = @event?.TryReserveSeats(1000);

        // Assert
        Assert.False(res);
    }

    public static IEnumerable<object[]> WrongEventParams() =>
        [
            [string.Empty, null, null, 0],
            ["test", null, null, 0],
            ["test", new DateTime(2026, 4, 1), null, 0],
            ["test", new DateTime(2026, 4, 1), new DateTime(2026, 3, 1), 0],
            ["test", new DateTime(2026, 2, 1), new DateTime(2026, 3, 1), 0],
        ];
}
