using EventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// DB конфигурация Booking
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .ValueGeneratedNever();
        builder.Property(b => b.EventId)
            .IsRequired();
        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>();
        builder.Property(b => b.CreatedAt)
            .IsRequired();
        builder.HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
