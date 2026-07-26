namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Исключение "Бронирование прошедшего события"
/// </summary>
public class PastEventBookingException : Exception
{
    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public PastEventBookingException() : base() { }

    /// <summary>
    /// Конструктор с сообщением
    /// </summary>
    /// <param name="message">Сообщение</param>
    public PastEventBookingException(string message)
        : base(message) { }

    /// <summary>
    /// Конструктор с сообщением и внутренним исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public PastEventBookingException(string message, Exception innerException)
        : base(message, innerException) { }
}