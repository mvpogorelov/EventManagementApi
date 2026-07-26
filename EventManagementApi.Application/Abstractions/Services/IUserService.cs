using EventManagement.Domain.Common;
using EventManagement.Domain.Entities;

namespace EventManagement.Application.Abstractions.Services;

internal interface IUserService
{
    Task<User> RegisterAsync(string login, string password, UserRole role, CancellationToken ct);
}
