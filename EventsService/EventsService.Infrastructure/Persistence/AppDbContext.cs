using EventManagement.Shared.Entities;
using EventsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsService.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Inbox> Inbox => Set<Inbox>();
    public DbSet<Outbox> Outbox => Set<Outbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
