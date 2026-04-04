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
    [Required(ErrorMessage = "Название должено быть заполнено")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание события
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Дата начала
    /// </summary>
    [Required(ErrorMessage = "Дата начала должна быть заполнена")]
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// Дата окончания
    /// </summary>
    [Required(ErrorMessage = "Дата окончания должна быть заполнена")]
    public DateTime? EndAt { get; set; }
}
