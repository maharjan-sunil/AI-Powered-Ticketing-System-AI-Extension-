using System.ComponentModel;

namespace AI_Powered_Ticketing_and_Support_System.Feature.TriageAutomation
{
    public enum TicketCategory { AccountAccess = 0, Technical = 1, Billing = 2, Network = 3 }
    public enum SentimentType { Positive = 0, Neutral = 1, Negative = 2, Angry = 3, Frustrated = 4 }

    // The target model that the AI must populate via function calling
    public record TriageReport(
        [property: Description("The determined category of the ticket.")]
    TicketCategory Category,

        [property: Description("The urgency score from 1 (low) to 5 (critical).")]
    int Urgency,

        [property: Description("The emotional sentiment detected in the ticket text.")]
    SentimentType Sentiment
    );
}
