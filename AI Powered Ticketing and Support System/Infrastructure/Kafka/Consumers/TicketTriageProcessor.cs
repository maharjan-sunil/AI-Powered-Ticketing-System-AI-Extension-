using AI_Powered_Ticketing_and_Support_System.Events;
using AI_Powered_Ticketing_and_Support_System.Feature.TriageAutomation;
using AI_Powered_Ticketing_and_Support_System.Infrastructure.Kafka.Consumers;
using AI_Powered_Ticketing_and_Support_System.Infrastructure.Kafka.Producers;

public class TicketTriageProcessor : ITicketTriageProcessor
{
    private readonly ILogger<TicketTriageProcessor> _logger;
    private readonly TicketAutomationService _aiService;
    private readonly TicketTriageProducer _producer;

    public TicketTriageProcessor(
        ILogger<TicketTriageProcessor> logger,
        TicketAutomationService ticketAutomationService,
        TicketTriageProducer producer)
    {
        _logger = logger;
        _aiService = ticketAutomationService;
        _producer = producer;
    }

    public async Task ProcessTicketAsync(
        TicketCreatedEvent ticketEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing ticket {TicketId}",
            ticketEvent.TicketId);

        var triageModel =
            await _aiService.ExtractTriageDetailsWithFunctionCallingAsync(
                ticketEvent.Description);

        var triageEvent = new TicketClassifiedEvent
        {
            TicketId = ticketEvent.TicketId,
            Category = triageModel.Category.ToString(),
            Priority = triageModel.Urgency.ToString(),
            Sentiment = triageModel.Sentiment.ToString()
        };

        await _producer.PublishTriageResultAsync(triageEvent);

        _logger.LogInformation(
            "Triage result published for ticket {TicketId}",
            ticketEvent.TicketId);
    }
}