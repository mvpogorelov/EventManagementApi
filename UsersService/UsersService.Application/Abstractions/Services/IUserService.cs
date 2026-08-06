using EventManagement.Contracts.Common;
using UsersService.Domain.Entities;

namespace UsersService.Application.Abstractions.Services;

public interface IUserService
{
    Task<User> RegisterAsync(string login, string password, UserRole role, CancellationToken ct);

    Task<string> LoginAsync(string login, string password, CancellationToken ct);
}
