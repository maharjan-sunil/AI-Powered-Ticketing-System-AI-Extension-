using AI_Powered_Ticketing_and_Support_System.Events;
using AI_Powered_Ticketing_and_Support_System.Infrastructure.Kafka.Consumers;
using Confluent.Kafka;
using System.Text.Json;

public class TicketCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TicketCreatedConsumer> _logger;

    public TicketCreatedConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<TicketCreatedConsumer> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = "ai-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer =
            new ConsumerBuilder<string, string>(config).Build();

        consumer.Subscribe("ticket-created");

        _logger.LogInformation("Kafka consumer started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);

                var ticketEvent =
                    JsonSerializer.Deserialize<TicketCreatedEvent>(
                        result.Message.Value);

                if (ticketEvent is null)
                    continue;

                // Create a new DI scope for processing
                using var scope = _scopeFactory.CreateScope();

                var processor = scope.ServiceProvider
                    .GetRequiredService<ITicketTriageProcessor>();

                await processor.ProcessTicketAsync(
                    ticketEvent,
                    stoppingToken);

                consumer.Commit(result);

                _logger.LogInformation(
                    "Ticket {TicketId} processed successfully.",
                    ticketEvent.TicketId);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer stopping.");
        }
        finally
        {
            consumer.Close();
        }
    }
}