using System.ComponentModel.DataAnnotations;

namespace EventManagmentApi.Presentation.Dto;

/// <summary>
/// DTO события
/// </summary>
public class EventDto
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
}
