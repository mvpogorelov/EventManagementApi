using System.ComponentModel.DataAnnotations;

namespace EventManagmentApi.Presentation.Dto;

public class EventDto
{
    [Required(ErrorMessage = "Название должено быть заполнено")]
    public string Title { get; set; }

    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Дата начала должна быть заполнена")]
    public DateTime? StartAt { get; set; }

    [Required(ErrorMessage = "Дата окончания должна быть заполнена")]
    public DateTime? EndAt { get; set; }
}
