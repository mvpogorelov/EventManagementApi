using EventManagement.Domain.Common;
using System.Text.Json.Serialization;

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
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole Role { get; set; } = UserRole.User;
}
