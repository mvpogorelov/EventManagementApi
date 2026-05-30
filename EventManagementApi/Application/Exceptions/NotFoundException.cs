namespace EventManagmentApi.Application.Exceptions;

/// <summary>
/// Исключение "Не найдено"
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public NotFoundException() : base() { }

    /// <summary>
    /// Конструктор с сообщением
    /// </summary>
    /// <param name="message">Сообщение</param>
    public NotFoundException(string message)
        : base(message) { }

    /// <summary>
    /// Конструктор с сообщением и внутренним исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}