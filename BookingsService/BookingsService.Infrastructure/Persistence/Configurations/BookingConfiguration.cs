using BookingsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingsService.Infrastructure.Persistence.Configurations;

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
        builder.Property(b => b.UserId)
            .IsRequired();
    }
}
