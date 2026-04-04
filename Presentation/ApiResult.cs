using System.Net;

namespace EventManagmentApi.Presentation;

/// <summary>
/// Результат работы API c возвращаемыми данными
/// </summary>
/// <typeparam name="T">Тип возвращаемых данных</typeparam>
public class ApiResult<T> : ApiResult
{
    /// <summary>
    /// Возвращаемые данные
    /// </summary>
    public required T Data { get; set; }
}

/// <summary>
/// Результат работы API без возвращаемых данных
/// </summary>
public class ApiResult : ApiResultBase { }

/// <summary>
/// Базовый класс результата работы API
/// </summary>
public class ApiResultBase
{
    /// <summary>
    /// Флаг, указывающий на успешность выполненного запроса
    /// </summary>
    public required bool Success { get; set; }

    /// <summary>
    /// Возвращаемый HTTP-код
    /// </summary>
    public required HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Дата и время ответа
    /// </summary>
    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cообщение с дополнительной информацией
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
