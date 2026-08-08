namespace EventManagement.Shared.Models;

public class KafkaSettings
{
    public required string BootstrapServers { get; set; }
    public required string ConsumerGroupId { get; set; }
    public required string ComsumerTopic { get; set; }

    public required KafkaTopics Topics { get; set; }
}
