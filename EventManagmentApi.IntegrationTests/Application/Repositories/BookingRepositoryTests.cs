using EventManagement.Domain.Common;
using EventManagement.Domain.Entities;
using EventManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventManagmentApi.IntegrationTests.Application.Repositories;

[Collection("Integration Tests")]
public class BookingRepositoryTests(DatabaseFixture databaseFixture)
{
    [Fact(DisplayName = "Корректный поиск брони по статусу")]
    public async Task GetByStatus_ShouldFindCorrectBooking()
    {
        // Arrange
        await databaseFixture.ResetDatabaseAsync();

        await using var context = databaseFixture.CreateContext();
        var @event = new Event("Title", DateTime.SpecifyKind(new DateTime(2026, 4, 1), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2026, 4, 10), DateTimeKind.Utc), 10, "Description");

        context.Events.Add(@event);
        await context.SaveChangesAsync();

        var booking1 = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = @event.Id,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        var booking2 = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = @event.Id,
            Status = BookingStatus.Rejected,
            CreatedAt = DateTime.UtcNow
        };
        var booking3 = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = @event.Id,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        context.Bookings.AddRange(booking1, booking2, booking3);
        await context.SaveChangesAsync();

        // Act
        await using var verifyContext = databaseFixture.CreateContext();
        var repository = new BookingRepository(verifyContext);
        var verifyBooks1 = await repository.GetByStatusAsync(BookingStatus.Pending);
        var verifyBooks2 = await repository.GetByStatusAsync(BookingStatus.Rejected);
        var verifyBooks3 = await repository.GetByStatusAsync(BookingStatus.Confirmed);

        // Assert
        Assert.Single(verifyBooks1);
        Assert.Equal(booking1.Id, verifyBooks1[0].Id);

        Assert.Single(verifyBooks2);
        Assert.Equal(booking2.Id, verifyBooks2[0].Id);

        Assert.Single(verifyBooks3);
        Assert.Equal(booking3.Id, verifyBooks3[0].Id);
    }
    
    [Fact(DisplayName = "Корректный поиск брони по id")]
    public async Task GetById_ShouldFindCorrectBooking()
    {
        // Arrange
        await databaseFixture.ResetDatabaseAsync();

        await using var context = databaseFixture.CreateContext();
        var @event = new Event("Title", DateTime.SpecifyKind(new DateTime(2026, 4, 1), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2026, 4, 10), DateTimeKind.Utc), 10, "Description");

        context.Events.Add(@event);
        await context.SaveChangesAsync();

        var booking1Id = Guid.NewGuid();
        var booking1 = new Booking
        {
            Id = booking1Id,
            EventId = @event.Id,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        var booking2Id = Guid.NewGuid();
        var booking2 = new Booking
        {
            Id = booking2Id,
            EventId = @event.Id,
            Status = BookingStatus.Rejected,
            CreatedAt = DateTime.UtcNow
        };

        context.Bookings.AddRange(booking1, booking2);
        await context.SaveChangesAsync();

        // Act
        await using var verifyContext = databaseFixture.CreateContext();
        var repository = new BookingRepository(verifyContext);
        var verifyBook1 = await repository.GetByIdAsync(booking1Id);
        var verifyBook2 = await repository.GetByIdAsync(booking2Id);
        var verifyBook3 = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(booking1Id, verifyBook1?.Id);
        Assert.Equal(booking2Id, verifyBook2?.Id);
        Assert.Null(verifyBook3);
    }
    
    [Fact(DisplayName = "Создание брони")]
    public async Task CreateAsync()
    {
        // Arrange
        await databaseFixture.ResetDatabaseAsync();

        await using var context = databaseFixture.CreateContext();
        var @event = new Event("Title", DateTime.SpecifyKind(new DateTime(2026, 4, 1), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2026, 4, 10), DateTimeKind.Utc), 10, "Description");

        context.Events.Add(@event);
        await context.SaveChangesAsync();

        var bookingId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = bookingId,
            EventId = @event.Id,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        var repository = new BookingRepository(context);

        // Act
        await repository.CreateAsync(booking);

        // Assert
        await using var verifyContext = databaseFixture.CreateContext();
        var savedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

        Assert.NotNull(savedBooking);
        Assert.Equal(bookingId, savedBooking.Id);
    }
    
    
    [Fact(DisplayName = "Создание дубликата брони, должна выбрасываться ошибка")]
    public async Task CreateAsync_IfDublicate_ShouldThrowError()
    {
        // Arrange
        await databaseFixture.ResetDatabaseAsync();

        await using var context = databaseFixture.CreateContext();
        var @event = new Event("Title", DateTime.SpecifyKind(new DateTime(2026, 4, 1), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2026, 4, 10), DateTimeKind.Utc), 10, "Description");

        context.Events.Add(@event);
        await context.SaveChangesAsync();

        var bookingId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = bookingId,
            EventId = @event.Id,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        var dublicate = new Booking
        {
            Id = bookingId,
            EventId = @event.Id,
            Status = BookingStatus.Rejected,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        await using var verifyContext = databaseFixture.CreateContext();
        var repository = new BookingRepository(verifyContext);

        await Assert.ThrowsAsync<DbUpdateException>(async () => await repository.CreateAsync(dublicate));
    }

    [Fact(DisplayName = "Обновление брони")]
    public async Task UpdateAsync()
    {
        // Arrange
        await databaseFixture.ResetDatabaseAsync();

        await using var context = databaseFixture.CreateContext();
        var @event = new Event("Title", DateTime.SpecifyKind(new DateTime(2026, 4, 1), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2026, 4, 10), DateTimeKind.Utc), 10, "Description");

        context.Events.Add(@event);
        await context.SaveChangesAsync();

        var bookingId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = bookingId,
            EventId = @event.Id,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        // Act
        await using var actContext = databaseFixture.CreateContext();
        var repository = new BookingRepository(actContext);
        var bookingUpdate = new Booking
        {
            Id = bookingId,
            EventId = @event.Id,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        await repository.UpdateAsync(bookingUpdate);

        // Assert
        await using var verifyContext = databaseFixture.CreateContext();
        var savedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

        Assert.NotNull(savedBooking);
        Assert.Equal(BookingStatus.Confirmed, savedBooking.Status);
    }

    [Fact(DisplayName = "Удаление брони")]
    public async Task DeleteAsync()
    {
        // Arrange
        await databaseFixture.ResetDatabaseAsync();

        await using var context = databaseFixture.CreateContext();
        var @event = new Event("Title", DateTime.SpecifyKind(new DateTime(2026, 4, 1), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2026, 4, 10), DateTimeKind.Utc), 10, "Description");

        context.Events.Add(@event);
        await context.SaveChangesAsync();

        var bookingId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = bookingId,
            EventId = @event.Id,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();

        // Act
        await using var actContext = databaseFixture.CreateContext();
        var repository = new BookingRepository(actContext);

        await repository.DeleteAsync(booking);

        // Assert
        await using var verifyContext = databaseFixture.CreateContext();
        var savedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

        Assert.Null(savedBooking);
    }


}
