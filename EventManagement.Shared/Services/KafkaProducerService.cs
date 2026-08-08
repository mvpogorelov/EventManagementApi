using Confluent.Kafka;
using EventManagement.Shared.Abstractions;
using EventManagement.Shared.Models;
using System.Text;
using System.Text.Json;

namespace EventManagement.Shared.Services;

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducerService(KafkaSettings settings)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, string key, T message, CancellationToken ct) where T : class
    {
        var messageValue = GetMessageString(message);
        var messageType = GetMessageType(message);

        await PublishAsync(topic, key, messageValue, messageType, ct);
    }

    public async Task PublishAsync(string topic, string key, string message, string messageType, CancellationToken ct)
    {
        var kafkaHeaders = new Headers
        {
            { "message-type", Encoding.UTF8.GetBytes(messageType) }
        };

        var kafkaMessage = new Message<string, string>
        {
            Key = key,
            Value = message,
            Headers = kafkaHeaders
        };

        await _producer.ProduceAsync(topic, kafkaMessage, ct);
    }

    public string GetMessageString<T>(T message) where T : class => JsonSerializer.Serialize(message);

    public string GetMessageType<T>(T message) where T : class => typeof(T).Name;
    
    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}
