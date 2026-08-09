using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsersService.Domain.Entities;

namespace UsersService.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedNever();
        builder.Property(u => u.Login)
            .IsRequired()
            .HasMaxLength(30);
        builder.HasIndex(u => u.Login)
            .IsUnique();
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(64);
        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>();
    }
}
