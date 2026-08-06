using BookingsService.Application.Abstractions.Services;
using Confluent.Kafka;
using System.Text;
using System.Text.Json;

namespace BookingsService.Infrastructure.Services;

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducerService(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishAsync<T>(
        string topic,
        string key,
        T message) where T : class
    {
        var jsonValue = JsonSerializer.Serialize(message);
        var kafkaHeaders = new Headers
        {
            { "message-type", Encoding.UTF8.GetBytes(typeof(T).Name) }
        };

        var kafkaMessage = new Message<string, string>
        {
            Key = key,
            Value = jsonValue,
            Headers = kafkaHeaders
        };

        await _producer.ProduceAsync(topic, kafkaMessage);
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}
