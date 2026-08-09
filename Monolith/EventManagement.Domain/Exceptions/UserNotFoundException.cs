namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Исключение "Пользователь не найден"
/// </summary>
public class UserNotFoundException : Exception
{
    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public UserNotFoundException() : base() { }

    /// <summary>
    /// Конструктор с сообщением
    /// </summary>
    /// <param name="message">Сообщение</param>
    public UserNotFoundException(string message)
        : base(message) { }

    /// <summary>
    /// Конструктор с сообщением и внутренним исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public UserNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}