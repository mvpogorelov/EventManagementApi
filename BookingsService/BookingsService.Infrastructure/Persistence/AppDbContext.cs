using BookingsService.Domain.Entities;
using EventManagement.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingsService.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Inbox> Inbox => Set<Inbox>();
    public DbSet<Outbox> Outbox => Set<Outbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
