using EventManagement.Application.Abstractions.Persistence.Repositories;
using EventManagement.Application.Abstractions.Security;
using EventManagement.Application.Abstractions.Services;
using EventManagement.Domain.Common;
using EventManagement.Domain.Entities;
using EventManagement.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Application.Services;

public class UserService(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IJwtService jwtService)
        : IUserService
{
    private const int LoginMaxLength = 30;

    public async Task<User> RegisterAsync(string login, string password, UserRole role, CancellationToken ct)
    {
        ValidateUserDataAndThrow(login, password);

        var existingUser = await userRepository.GetByLoginAsync(login, ct);

        if (existingUser is not null)
        {
            throw new ValidationException($"Пользователь уже существует");
        }

        var user = new User(login, passwordService.Hash(password), role);

        return await userRepository.CreateAsync(user, ct);
    }

    public async Task<string> LoginAsync(string login, string password, CancellationToken ct)
    {
        var user = await userRepository.GetByLoginAsync(login, ct);

        if (user is null || !passwordService.Verify(password, user.PasswordHash))
        {
            throw new UserNotFoundException($"Ошибка входа");
        }

        return jwtService.GenerateToken(user);
    }

    private void ValidateUserDataAndThrow(string login, string password)
    {
        if (string.IsNullOrEmpty(login))
        {
            throw new ValidationException($"Логин не может быть пустым");
        }

        if (login.Length > LoginMaxLength)
        {
            throw new ValidationException($"Логин не может быть диннее {LoginMaxLength} символов");
        }

        if (string.IsNullOrEmpty(password))
        {
            throw new ValidationException($"Пароль не может быть пустым");
        }
    }
}
