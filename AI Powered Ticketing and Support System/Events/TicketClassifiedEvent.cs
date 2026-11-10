namespace AI_Powered_Ticketing_and_Support_System.Events
{
    public class TicketClassifiedEvent
    {

        public Guid EventId { get; set; } = Guid.NewGuid();
        public Guid TicketId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
    }
}
