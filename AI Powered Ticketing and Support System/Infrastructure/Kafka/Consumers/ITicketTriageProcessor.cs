using AI_Powered_Ticketing_and_Support_System.Events;

namespace AI_Powered_Ticketing_and_Support_System.Infrastructure.Kafka.Consumers
{
    public interface ITicketTriageProcessor
    {
        Task ProcessTicketAsync(
        TicketCreatedEvent ticketEvent,
        CancellationToken cancellationToken);
    }
}
