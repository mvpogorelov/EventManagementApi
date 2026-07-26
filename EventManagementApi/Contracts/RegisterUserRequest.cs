using EventManagement.Domain.Common;

namespace EventManagement.Presentation.Contracts;

/// <summary>
/// 
/// </summary>
public class RegisterUserRequest
{
    /// <summary>
    /// Логин
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Роль
    /// </summary>
    public UserRole Role { get; set; } = UserRole.User;
}
