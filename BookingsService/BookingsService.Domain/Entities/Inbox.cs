using BookingsService.Domain.Common;
using System.Diagnostics.CodeAnalysis;

namespace BookingsService.Domain.Entities;

/// <summary>
/// Сущность для храния сообщений Kafka
/// </summary>
public sealed class Inbox
{
    public int Id { get; set; }
    public required string Topic { get; set; }
    public required string MessageKey { get; set; }
    public required string Message { get; set; }
    public string? MessageType { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
    public required InboxStatus Status { get; set; }
    public string? StatusComment { get; set; }

    [SetsRequiredMembers]
    public Inbox(string topic,
        string messageKey,
        string message,
        DateTimeOffset processedAt,
        InboxStatus status,
        string? messageType = null,
        string? statusComment = null)
    {
        Topic = topic;
        MessageKey = messageKey;
        Message = message;
        ProcessedAt = processedAt;
        Status = status;
        MessageType = messageType;
        StatusComment = statusComment;
    }
}
