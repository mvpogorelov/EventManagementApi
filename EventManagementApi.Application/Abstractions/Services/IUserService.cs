using EventManagement.Domain.Common;
using EventManagement.Domain.Entities;

namespace EventManagement.Application.Abstractions.Services;

public interface IUserService
{
    Task<User> RegisterAsync(string login, string password, UserRole role, CancellationToken ct);

    Task<string> LoginAsync(string login, string password, CancellationToken ct);
}
