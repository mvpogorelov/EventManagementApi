namespace EventsService.Domain.Exceptions;

/// <summary>
/// Исключение "Превышение лимита активных броней"
/// </summary>
public class BookingLimitException : Exception
{
    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public BookingLimitException() : base() { }

    /// <summary>
    /// Конструктор с сообщением
    /// </summary>
    /// <param name="message">Сообщение</param>
    public BookingLimitException(string message)
        : base(message) { }

    /// <summary>
    /// Конструктор с сообщением и внутренним исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public BookingLimitException(string message, Exception innerException)
        : base(message, innerException) { }
}
