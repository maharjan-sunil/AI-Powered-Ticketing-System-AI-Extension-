namespace AI_Powered_Ticketing_and_Support_System.Events
{
    public class TicketCreatedEvent
    {
       
            public Guid EventId { get; set; } = Guid.NewGuid();
            public Guid TicketId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
    }
}
