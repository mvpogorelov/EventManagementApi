namespace EventManagement.Presentation.Contracts;

/// <summary>
/// DTO брони для ответов
/// </summary>
public record BookingOutDto(Guid Id, Guid EventId, string Status);
