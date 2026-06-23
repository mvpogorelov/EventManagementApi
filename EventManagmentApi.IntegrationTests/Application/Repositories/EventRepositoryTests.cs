using EventManagmentApi.Application.Enums;
using EventManagmentApi.Application.Models;
using EventManagmentApi.Application.Repositories;
using EventManagmentApi.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventManagmentApi.IntegrationTests.Application.Repositories;

public class EventRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();
    private Event? event1, event2, event3, event4;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        var context = new AppDbContext(options);

        context.Database.Migrate();

        return context;
    }

    private async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();
        await context.Database.ExecuteSqlRawAsync(@"
            DO $$ DECLARE
                r RECORD;
            BEGIN
                FOR r IN (SELECT tablename FROM pg_tables 
                          WHERE schemaname = 'public' 
                            AND tablename NOT IN ('__EFMigrationsHistory', 'schema_version')) 
                LOOP
                    EXECUTE 'TRUNCATE TABLE ' || quote_ident(r.tablename) || ' RESTART IDENTITY CASCADE;';
                END LOOP;
            END $$;");
    }

    private async Task SetTestData(CancellationToken ct = default)
    {
        event1 = new Event("Aa", DateTime.SpecifyKind(new DateTime(2026, 4, 1), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2026, 4, 10), DateTimeKind.Utc), 2, "AAaa");
        event2 = new Event("Bb", DateTime.SpecifyKind(new DateTime(2026, 3, 1), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2026, 3, 10), DateTimeKind.Utc), 1, "BBbb");
        event3 = new Event("Cc", DateTime.SpecifyKind(new DateTime(2026, 2, 1), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2026, 2, 10), DateTimeKind.Utc), 1, "CCcc");
        event4 = new Event("Ccc", DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2026, 1, 10), DateTimeKind.Utc), 1, "CCCccc");
        await using var context = CreateContext();

        await context.Events.AddRangeAsync(event1, event2, event3, event4);
        await context.SaveChangesAsync(ct);
    }

    [Fact(DisplayName = "Получение списка событий: Если фильтры не заданы, то должны вернуться все события")]
    public async Task GetPaginatedAsync_WhenFiltersAreNotDefined_ShouldReturnAllEvents()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SetTestData();
        await using var context = CreateContext();
        var repository = new EventRepository(context);

        // Act
        var events = await repository.GetPaginatedAsync();

        // Assert
        Assert.Equal(4, events.Items.Count);
        Assert.Equal(1, events.Page);
        Assert.Equal(4, events.PageSize);
        Assert.Equal(4, events.TotalItems);
        Assert.Equal(1, events.TotalPages);
    }

    [Theory(DisplayName = "Получение списка событий: Если заданы параметры пагинации, то должны вернуться соответствующие данные")]
    [InlineData(1, 2, 2, 4, 2)]
    [InlineData(2, 1, 1, 4, 4)]
    [InlineData(1, 4, 4, 4, 1)]
    public async Task GetPaginatedAsync_WhenPaginationParamsAreDefined_ShouldReturnCorrectData(int page, int pageSize, int expectedItemsCount, int expectedTotalItems, int expectedTotalPages)
    {
        // Arrange
        await ResetDatabaseAsync();
        await SetTestData();
        await using var context = CreateContext();
        var repository = new EventRepository(context);

        // Act
        var events = await repository.GetPaginatedAsync(page: page, pageSize: pageSize);

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
    public async Task GetPaginatedAsync_WhenTitleIsDefined_ShouldReturnCorrectEvents(string title)
    {
        // Arrange
        await ResetDatabaseAsync();
        await SetTestData();
        await using var context = CreateContext();
        var repository = new EventRepository(context);

        // Act
        var events = await repository.GetPaginatedAsync(title);

        // Assert
        Assert.Single(events.Items);
    }

    [Fact(DisplayName = "Получение списка событий: Если задан фильтр по дате начала, то должны вернуться соотвествующие события")]
    public async Task GetPaginatedAsync_WhenStartAtIsDefined_ShouldReturnCorrectEvents()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SetTestData();
        await using var context = CreateContext();
        var repository = new EventRepository(context);
        var startAt = DateTime.SpecifyKind(new DateTime(2026, 3, 1), DateTimeKind.Utc);

        // Act
        var events = await repository.GetPaginatedAsync(from: startAt);

        // Assert
        Assert.Equal(2, events.Items.Count);
    }

    [Fact(DisplayName = "Получение списка событий: Если задан фильтр по дате окончания, то должны вернуться соотвествующие события")]
    public async Task GetPaginatedAsync_WhenEndAtIsDefined_ShouldReturnCorrectEvents()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SetTestData();
        await using var context = CreateContext();
        var repository = new EventRepository(context);
        var endAt = DateTime.SpecifyKind(new DateTime(2026, 3, 10), DateTimeKind.Utc);

        // Act
        var events = await repository.GetPaginatedAsync(to: endAt);

        // Assert
        Assert.Equal(3, events.Items.Count);
    }

    [Fact(DisplayName = "Получение списка событий: Если задано несколько фильтров, то должны вернуться соотвествующие события")]
    public async Task GetPaginatedAsync_WhenFiltersAreDefined_ShouldReturnCorrectEvents()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SetTestData();
        await using var context = CreateContext();
        var repository = new EventRepository(context);

        var title = "cc";
        var startAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc);
        var endAt = DateTime.SpecifyKind(new DateTime(2026, 3, 1), DateTimeKind.Utc);

        // Act
        var events = await repository.GetPaginatedAsync(title, startAt, endAt);

        // Assert
        Assert.Equal(2, events.Items.Count);
    }

    [Fact(DisplayName = "Корректный поиск события по id")]
    public async Task GetById_ShouldFindCorrectBooking()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SetTestData();
        await using var context = CreateContext();

        // Act
        await using var verifyContext = CreateContext();
        var repository = new EventRepository(verifyContext);
        var verifyEvent1 = await repository.GetByIdAsync(event1.Id);
        var verifyEvent2 = await repository.GetByIdAsync(event2.Id);
        var verifyEvent3 = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(event1.Id, verifyEvent1?.Id);
        Assert.Equal(event2.Id, verifyEvent2?.Id);
        Assert.Null(verifyEvent3);
    }

    [Fact(DisplayName = "Создание события")]
    public async Task CreateAsync()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = CreateContext();
        var repository = new EventRepository(context);
        var @event = new Event("Title", DateTime.SpecifyKind(new DateTime(2026, 4, 1), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2026, 4, 10), DateTimeKind.Utc), 10, "Description");

        // Act
        await repository.CreateAsync(@event);

        // Assert
        await using var verifyContext = CreateContext();
        var saved = await verifyContext.Events.FirstOrDefaultAsync();

        Assert.NotNull(saved);
        Assert.Equal(@event.Id, saved.Id);
    }

    [Fact(DisplayName = "Создание события c очень длинным заголовком, должна выбрасываться ошибка")]
    public async Task CreateAsync_IfTitleMoreThen300Symbols_ShouldThrowError()
    {
        // Arrange
        await ResetDatabaseAsync();

        await using var context = CreateContext();
        var repository = new EventRepository(context);
        var @event = new Event("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890",
            DateTime.SpecifyKind(new DateTime(2026, 4, 1), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2026, 4, 10), DateTimeKind.Utc), 10, "Description");

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () => await repository.CreateAsync(@event));
    }


    [Fact(DisplayName = "Обновление события")]
    public async Task UpdateAsync()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SetTestData();
        await using var context = CreateContext();
        var repository = new EventRepository(context);

        // Act
        event1?.Title = "Title";
        await repository.UpdateAsync(event1);

        // Assert
        await using var verifyContext = CreateContext();
        var saved = await verifyContext.Events.FirstOrDefaultAsync(e => e.Id == event1.Id);

        Assert.Equal("Title", saved.Title);
    }
    
    
    [Fact(DisplayName = "Удаление события, должны удаляться связанные брони")]
    public async Task DeleteAsync()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SetTestData();
        await using var context = CreateContext();
        var repository = new EventRepository(context);
        var bookingId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = bookingId,
            EventId = event1.Id,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(event1);

        // Assert
        await using var verifyContext = CreateContext();
        var saved = await verifyContext.Events.FirstOrDefaultAsync(e => e.Id == event1.Id);
        var savedBookings = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

        Assert.Null(saved);
        Assert.Null(savedBookings);
    }
    
    
    [Fact(DisplayName = "Удаление всех событий")]
    public async Task DeleteAllAsync()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SetTestData();
        await using var context = CreateContext();
        var repository = new EventRepository(context);

        // Act
        await repository.DeleteAllAsync();

        // Assert
        await using var verifyContext = CreateContext();
        var saved = await verifyContext.Events.ToArrayAsync();

        Assert.Empty(saved);
    }
}
