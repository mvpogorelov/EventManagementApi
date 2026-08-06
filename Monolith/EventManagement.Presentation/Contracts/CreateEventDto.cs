namespace EventManagement.Presentation.Contracts;

/// <summary>
/// DTO события
/// </summary>
public class CreateEventDto
{
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
}
