using EventManagement.Domain.Common;
using System.Diagnostics.CodeAnalysis;

namespace EventManagement.Domain.Entities;

public record User
{
    [SetsRequiredMembers]
    public User(string login, string passwordHash, UserRole role = UserRole.User)
    {
        Id = Guid.NewGuid();
        Login = login;
        PasswordHash = passwordHash;
        Role = role;
    }
    public required Guid Id { get; init; }
    public required string Login { get; set; }
    public required string PasswordHash { get; set; }
    public required UserRole Role { get; set; }
    public ICollection<Booking> Bookings { get; set; } = [];
}
