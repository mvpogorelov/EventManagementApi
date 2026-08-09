namespace EventManagement.Presentation.Contracts;

/// <summary>
/// 
/// </summary>
public class EventInfoDto
{
    /// <summary>
    /// Уникальный идентификатор события
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Название события
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание события
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Дата начала
    /// </summary>
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// Дата окончания
    /// </summary>
    public DateTime? EndAt { get; set; }

    /// <summary>
    /// Общее количество мест на событии
    /// </summary>
    public int TotalSeats { get; set; }

    /// <summary>
    /// Текущее количество свободных мест
    /// </summary>
    public int AvailableSeats { get; set; }
}
