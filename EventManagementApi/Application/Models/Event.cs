using EventManagmentApi.Application.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace EventManagmentApi.Application.Models;

/// <summary>
/// Модель события
/// </summary>
public class Event
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="startAt"></param>
    /// <param name="endAt"></param>
    /// <param name="totalSeats"></param>
    /// <param name="description"></param>
    [SetsRequiredMembers]
    public Event(string title, DateTime startAt, DateTime endAt, int totalSeats, string? description = null)
    {
        Id = Guid.NewGuid();
        Title = title;
        StartAt = startAt;
        EndAt = endAt;
        Description = description;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
    }


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
    public required int TotalSeats {
        get;
        set
        {
            var diff = value - field;

            if (AvailableSeats + diff < 0)
            {
                throw new NoAvailableSeatsException($"Уже забронировано {AvailableSeats}. Общее количество {field} не может быть изменено на {value}");
            }

            field = value;
            AvailableSeats += diff;
        }
    }

    /// <summary>
    /// Текущее количество свободных мест
    /// </summary>
    public int AvailableSeats { get; private set; }

    /// <summary>
    /// Список брони
    /// </summary>
    public ICollection<Booking> Bookings { get; set; } = [];

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

    private Event() { }
}
