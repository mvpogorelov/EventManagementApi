using EventManagement.Presentation.Contracts;

namespace EventManagement.Presentation.Dto;

/// <summary>
/// Результат работы API с параметрами пагинации
/// </summary>
/// <typeparam name="T"></typeparam>
public class PaginatedResultDto<T> : ApiResultDto<T>
{
    /// <summary>
    /// Номер страницы
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Размер страницы
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Всего записей
    /// </summary>
    public int TotalItems { get; init; }

    /// <summary>
    /// Всего страниц
    /// </summary>
    public int TotalPages { get; init; }
}
