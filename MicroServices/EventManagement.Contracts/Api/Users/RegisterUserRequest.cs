using EventManagement.Contracts.Common;
using System.Text.Json.Serialization;

namespace EventManagement.Contracts.Api.Users;

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