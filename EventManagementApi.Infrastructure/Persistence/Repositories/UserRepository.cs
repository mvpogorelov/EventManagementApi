using EventManagement.Application.Abstractions.Persistence.Repositories;
using EventManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    /// <summary>
    /// Получение пользователя по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Пользователь</returns>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Users
            .Include(u => u.Bookings)
            .ThenInclude(b => b.Event)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    /// <summary>
    /// Получение пользователя по логину
    /// </summary>
    /// <param name="login">Логин пользователя</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Пользователь</returns>
    public async Task<User?> GetByLoginAsync(string login, CancellationToken ct = default) =>
        await context.Users
            .Include(u => u.Bookings)
            .ThenInclude(b => b.Event)
            .FirstOrDefaultAsync(e => e.Login == login, ct);

    /// <summary>
    /// Создание пользователя
    /// </summary>
    /// <param name="user">Пользователь</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Пользователь</returns>
    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        await context.Users.AddAsync(user, ct);
        await context.SaveChangesAsync(ct);

        return user;
    }
}
