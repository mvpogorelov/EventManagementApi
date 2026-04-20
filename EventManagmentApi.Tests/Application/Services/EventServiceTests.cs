using EventManagmentApi.Application.Exceptions;
using EventManagmentApi.Application.Models;
using EventManagmentApi.Application.Services;
using static System.Net.WebRequestMethods;

namespace EventManagmentApi.Tests.Application.Services;

public class EventServiceTests
{
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        _eventService = new EventService();

        Dictionary<int, Event> events = new()
        {
            {1, new Event { Id = 1, Title = "Aa", Description = "AAaa", StartAt = new DateTime(2026, 4, 1), EndAt = new DateTime(2026, 4, 10) } },
            {2, new Event { Id = 2, Title = "Bb", Description = "BBbb", StartAt = new DateTime(2026, 3, 1), EndAt = new DateTime(2026, 3, 10) } },
            {3, new Event { Id = 3, Title = "Cc", Description = "CCcc", StartAt = new DateTime(2026, 2, 1), EndAt = new DateTime(2026, 2, 10) } },
            {4, new Event { Id = 4, Title = "Ccc", Description = "CCCccc", StartAt = new DateTime(2026, 1, 1), EndAt = new DateTime(2026, 1, 10) } },
        };

        _eventService.InitData(events, 5);
    }

    [Theory(DisplayName = "Создание: Если переданы неверные параметры, то должно выбрасываться исключение")]
    [MemberData(nameof(WrongEventParams))]
    public void Create_WhenParamsAreWrong_ShouldThrowException(string title, DateTime? startAt, DateTime? endAt)
    {
        // Act
        var ex = Record.Exception(() => _eventService.Create(title, startAt, endAt));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact(DisplayName = "Создание: Если параметры верны, то должно создаться событие")]
    public void Create_WhenParamsAreCorrect_ShouldCreateEvent()
    {
        // Arrange
        var title = "Title";
        var startAt = new DateTime(2026, 4, 1);
        var endAt = new DateTime(2026, 4, 10);
        var description = "Description";

        // Act
        var @event = _eventService.Create(title, startAt, endAt, description);

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
    public void GetAll_WhenParamsAreWrong_ShouldThrowException(int page, int pageSize)
    {
        // Act
        var ex = Record.Exception(() => _eventService.GetAll(page: page, pageSize: pageSize));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<ArgumentOutOfRangeException>(ex);
    }

    [Fact(DisplayName = "Получение списка событий: Если фильтры не заданы, то должны вернуться все события")]
    public void GetAll_WhenFiltersAreNotDefined_ShouldReturnAllEvents()
    {
        // Act
        var events = _eventService.GetAll();

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
    [InlineData(2, 3, 1, 4, 2)]
    [InlineData(1, 5, 4, 4, 1)]
    public void GetAll_WhenPaginationParamsAreDefined_ShouldReturnCorrectData(int page, int pageSize, int expectedItemsCount, int expectedTotalItems, int expectedTotalPages)
    {
        // Act
        var events = _eventService.GetAll(page: page, pageSize: pageSize);

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
    public void GetAll_WhenTitleIsDefined_ShouldReturnCorrectEvents(string title)
    {
        // Act
        var events = _eventService.GetAll(title);

        // Assert
        Assert.Single(events.Items);
    }

    [Fact(DisplayName = "Получение списка событий: Если задан фильтр по дате начала, то должны вернуться соотвествующие события")]
    public void GetAll_WhenStartAtIsDefined_ShouldReturnCorrectEvents()
    {
        // Arrange
        var startAt = new DateTime(2026, 3, 1);

        // Act
        var events = _eventService.GetAll(from: startAt);

        // Assert
        Assert.Equal(2, events.Items.Count);
    }
    
    [Fact(DisplayName = "Получение списка событий: Если задан фильтр по дате окончания, то должны вернуться соотвествующие события")]
    public void GetAll_WhenEndAtIsDefined_ShouldReturnCorrectEvents()
    {
        // Arrange
        var endAt = new DateTime(2026, 3, 10);

        // Act
        var events = _eventService.GetAll(to: endAt);

        // Assert
        Assert.Equal(3, events.Items.Count);
    }

    [Fact(DisplayName = "Получение списка событий: Если задано несколько фильтров, то должны вернуться соотвествующие события")]
    public void GetAll_WhenFiltersAreDefined_ShouldReturnCorrectEvents()
    {
        // Arrange
        var title = "cc";
        var startAt = new DateTime(2026, 1, 1);
        var endAt = new DateTime(2026, 3, 1);

        // Act
        var events = _eventService.GetAll(title, startAt, endAt);

        // Assert
        Assert.Equal(2, events.Items.Count);
    }

    [Theory(DisplayName = "Получение события по id: Если передан несущестующий id, то должно выбрасываться исключение")]
    [InlineData(6)]
    [InlineData(125)]
    public void Get_WhenIdIsIncorrect_ShouldThrowException(int id)
    {
        // Act
        var ex = Record.Exception(() => _eventService.Get(id));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<NotFoundException>(ex);
    }
    
    [Fact(DisplayName = "Получение события по id: Если передан сущестующий id, то должно вернуться событие")]
    public void Get_WhenIdIsСorrect_ShouldReturnEvent()
    {
        // Act
        var @event = _eventService.Get(1);

        // Assert
        Assert.NotNull(@event);
        Assert.Equal("Aa", @event.Title);
    }

    [Theory(DisplayName = "Обновление: Если переданы неверные параметры, то должно выбрасываться исключение")]
    [MemberData(nameof(WrongEventParams))]
    public void Update_WhenParamsAreWrong_ShouldThrowException(string title, DateTime? startAt, DateTime? endAt)
    {
        // Act
        var ex = Record.Exception(() => _eventService.Update(1, title, startAt, endAt));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
    }
    
    [Theory(DisplayName = "Обновление: Если переданы несуществующий id, то должно выбрасываться исключение")]
    [InlineData(13)]
    [InlineData(284)]
    public void Update_WhenIdIsMissing_ShouldThrowException(int id)
    {
        // Arrange
        var title = "Title";
        var startAt = new DateTime(2026, 1, 1);
        var endAt = new DateTime(2026, 3, 1);

        // Act
        var ex = Record.Exception(() => _eventService.Update(id, title, startAt, endAt));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<NotFoundException>(ex);
    }

    [Fact(DisplayName = "Обновление: Если переданы корректные данные, то должно обновится событие")]
    public void Update_WhenParamsAreСorrect_ShouldUpdateEvent()
    {
        // Arrange
        var id = 1;
        var title = "Title";
        var startAt = new DateTime(2026, 1, 1);
        var endAt = new DateTime(2026, 3, 1);

        // Act
        var ex = Record.Exception(() => _eventService.Update(id, title, startAt, endAt));

        // Assert
        Assert.Null(ex);
        // ToDo: добавить проверки, когда перейдём на другой источник данных
    }

    [Theory(DisplayName = "Удаление: Если переданы несуществующий id, то должно выбрасываться исключение")]
    [InlineData(13)]
    [InlineData(284)]
    public void Remove_WhenIdIsMissing_ShouldThrowException(int id)
    {
        // Act
        var ex = Record.Exception(() => _eventService.Remove(id));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<NotFoundException>(ex);
    }

    [Fact(DisplayName = "Удаление: Если переданы корректные данные, то должно удалиться событие")]
    public void Remove_WhenParamsAreСorrect_ShouldRemoveEvent()
    {
        // Arrange
        var id = 1;

        // Act
        var ex = Record.Exception(() => _eventService.Remove(id));

        // Assert
        Assert.Null(ex);
        // ToDo: добавить проверки, когда перейдём на другой источник данных
    }

    public static IEnumerable<object[]> WrongEventParams() =>
        [
            [string.Empty, null, null],
            ["test", null, null],
            ["test", new DateTime(2026, 4, 1), null],
            ["test", new DateTime(2026, 4, 1), new DateTime(2026, 3, 1)],
        ];
}
