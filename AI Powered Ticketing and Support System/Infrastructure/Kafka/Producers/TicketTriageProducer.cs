using Confluent.Kafka;
using System.Text.Json;
using AI_Powered_Ticketing_and_Support_System.Events;
using Microsoft.Extensions.Options;

namespace AI_Powered_Ticketing_and_Support_System.Infrastructure.Kafka.Producers
{
    public class TicketTriageProducer
    {
        private readonly IProducer<string, string> _producer;
        private readonly KafkaSettings _settings;

        public TicketTriageProducer(
            IOptions<KafkaSettings> settings)
        {
            _settings = settings.Value; 
            var config = new ProducerConfig
            {
                BootstrapServers = _settings.BootstrapServers
                    
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishTriageResultAsync(
            TicketClassifiedEvent triageEvent)
        {
            var message = new Message<string, string>
            {
                Key = triageEvent.TicketId.ToString(),
                Value = JsonSerializer.Serialize(triageEvent)
            };

            await _producer.ProduceAsync(
                _settings.TicketClassifiedTopic,
                message);
        }
    }
}