namespace EventManagement.Domain.Exceptions;

/// <summary>
/// Исключение "Не найдено"
/// </summary>
public class OperationNotAllowedException : Exception
{
    /// <summary>
    /// Операция не допустима
    /// </summary>
    public OperationNotAllowedException() : base() { }

    /// <summary>
    /// Конструктор с сообщением
    /// </summary>
    /// <param name="message">Сообщение</param>
    public OperationNotAllowedException(string message)
        : base(message) { }

    /// <summary>
    /// Конструктор с сообщением и внутренним исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Внутреннее исключение</param>
    public OperationNotAllowedException(string message, Exception innerException)
        : base(message, innerException) { }
}