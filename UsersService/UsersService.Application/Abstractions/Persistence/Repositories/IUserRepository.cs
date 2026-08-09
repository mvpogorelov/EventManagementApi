using UsersService.Domain.Entities;

namespace UsersService.Application.Abstractions.Persistence.Repositories;

public interface IUserRepository
{
    /// <summary>
    /// Получение пользователя по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Пользователь</returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Получение пользователя по логину
    /// </summary>
    /// <param name="login">Логин пользователя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Пользователь</returns>
    Task<User?> GetByLoginAsync(string login, CancellationToken ct = default);

    /// <summary>
    /// Создание пользователя
    /// </summary>
    /// <param name="user">Пользователь</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Пользователь</returns>
    Task<User> CreateAsync(User user, CancellationToken ct = default);
}
