using EventManagement.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsService.Infrastructure.Persistence.Configurations;

/// <summary>
/// DB конфигурация Outbox
/// </summary>
public class OutboxConfiguration : IEntityTypeConfiguration<Outbox>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Outbox> builder)
    {
        builder.ToTable("outbox");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Topic)
            .IsRequired();
        builder.Property(i => i.MessageKey)
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(i => i.Message)
            .IsRequired();
        builder.Property(i => i.MessageType)
            .IsRequired()
            .HasMaxLength(50);
    }
}
