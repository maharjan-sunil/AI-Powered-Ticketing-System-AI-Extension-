namespace AI_Powered_Ticketing_and_Support_System.Infrastructure.Kafka
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = "localhost:9092";

        public string TicketCreatedTopic { get; set; } = "ticket-created";

        public string TicketClassifiedTopic { get; set; } = "ticket-classified";
    }
}

