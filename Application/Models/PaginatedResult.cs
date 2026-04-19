namespace EventManagmentApi.Application.Models;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Items"></param>
/// <param name="Page"></param>
/// <param name="PageSize"></param>
/// <param name="TotalItems"></param>
/// <param name="TotalPages"></param>
public record PaginatedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);
