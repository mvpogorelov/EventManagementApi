namespace EventManagement.Presentation.Contracts;

/// <summary>
/// 
/// </summary>
public class LoginUserRequest
{
    /// <summary>
    /// Логин
    /// </summary>
    public string Login { get; set; } = string.Empty;
    
    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
