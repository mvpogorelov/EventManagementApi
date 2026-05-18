namespace EventManagmentApi.Application.Exceptions;

/// <summary>
/// Исключение "Не найдено"
/// </summary>
public class NoAvailableSeatsException : Exception
{
    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public NoAvailableSeatsException() : base() { }

    /// <summary>
    /// Конструктор с сообщением
    /// </summary>
    /// <param name="message">Сообщение</param>
    public NoAvailableSeatsException(string message)
        : base(message) { }

    /// <summary>
    /// Конструктор с сообщением и внутренним исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public NoAvailableSeatsException(string message, Exception innerException)
        : base(message, innerException) { }
}