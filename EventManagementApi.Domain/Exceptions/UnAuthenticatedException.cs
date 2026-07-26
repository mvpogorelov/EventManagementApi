namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Исключение "Не аутентифицирован"
/// </summary>
public class UnAuthenticatedException : Exception
{
    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public UnAuthenticatedException() : base() { }

    /// <summary>
    /// Конструктор с сообщением
    /// </summary>
    /// <param name="message">Сообщение</param>
    public UnAuthenticatedException(string message)
        : base(message) { }

    /// <summary>
    /// Конструктор с сообщением и внутренним исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public UnAuthenticatedException(string message, Exception innerException)
        : base(message, innerException) { }
}