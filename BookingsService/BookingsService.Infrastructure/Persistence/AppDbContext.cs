using BookingsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingsService.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<Inbox> Inbox => Set<Inbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
