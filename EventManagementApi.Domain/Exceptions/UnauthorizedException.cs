namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Исключение "Не аутентифицирован"
/// </summary>
public class UnauthorizedException : Exception
{
    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public UnauthorizedException() : base() { }

    /// <summary>
    /// Конструктор с сообщением
    /// </summary>
    /// <param name="message">Сообщение</param>
    public UnauthorizedException(string message)
        : base(message) { }

    /// <summary>
    /// Конструктор с сообщением и внутренним исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException) { }
}