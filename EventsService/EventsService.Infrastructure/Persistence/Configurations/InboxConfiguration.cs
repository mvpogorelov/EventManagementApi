using EventManagement.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsService.Infrastructure.Persistence.Configurations;

/// <summary>
/// DB конфигурация Inbox
/// </summary>
public class InboxConfiguration : IEntityTypeConfiguration<Inbox>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Inbox> builder)
    {
        builder.ToTable("inbox");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Topic)
            .IsRequired();
        builder.Property(i => i.MessageKey)
            .HasMaxLength(40)
            .IsRequired();
        builder.Property(i => i.Message)
            .IsRequired();
        builder.Property(i => i.MessageType)
            .HasMaxLength(50);
    }
}
