namespace EventManagmentApi.Application.Models;

/// <summary>
/// Модель события
/// </summary>
public record Event
{
    /// <summary>
    /// Уникальный идентификатор события
    /// </summary>
    public required Guid Id { get; init; }

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

    /// <summary>
    /// Попытка резервирования мест
    /// </summary>
    /// <param name="count">Количество мест</param>
    /// <returns>true - если удачно</returns>
    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats - count < 0)
        {
            return false;
        }

        AvailableSeats -= count;

        return true;
    }

    /// <summary>
    /// Освобождение мест для резервирования
    /// </summary>
    /// <param name="count">Количество мест для освобождения</param>
    public void ReleaseSeats(int count = 1)
    {
        AvailableSeats =
            AvailableSeats + count > TotalSeats
            ? TotalSeats
            : AvailableSeats + count;
    }
}
