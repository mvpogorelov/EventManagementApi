namespace EventManagement.Shared.Models;

public class KafkaSettings
{
    public required string BootstrapServers { get; set; }

    public required KafkaTopics Topics { get; set; }
}
