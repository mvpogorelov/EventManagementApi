namespace EventManagmentApi.Application.Models;

/// <summary>
/// Модель события
/// </summary>
public class Event
{
    /// <summary>
    /// Уникальный идентификатор события
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Название события
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Описание события
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата начала
    /// </summary>
    public required DateTime StartAt { get; set; }

    /// <summary>
    /// Дата окончания
    /// </summary>
    public required DateTime EndAt { get; set; }

    /// <summary>
    /// Общее количество мест на событии
    /// </summary>
    public required int TotalSeats { get; set; }

    /// <summary>
    /// Текущее количество свободных мест
    /// </summary>
    public int AvailableSeats { get; set; }
}
