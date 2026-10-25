using System.ComponentModel;

namespace AI_Powered_Ticketing_and_Support_System.Feature.TriageAutomation
{
    public class TicketTools
    {

        public TriageReport? LatestReport { get; private set; }

        [Description("Submits an automated ticket classification triage report consisting of urgency, category, and sentiment.")]
        public void SubmitTriageReport(
            TicketCategory category,
            int urgency,
            SentimentType sentiment)
        {
            LatestReport = new TriageReport(category, urgency, sentiment);
        }


        /*
         * Not needed as we already prompt AI to use the exact enum values for category and sentiment.
         * systemMessage in ChatClien
         */

        //    public void SubmitTriageReport(
        //string category,
        //int urgency,
        //string sentiment)
        //    {
        //        Console.WriteLine($"AI Category: {category}");

        //        var normalizedCategory = category
        //            .Replace(" ", "")
        //            .Replace("/", "")
        //            .Replace("-", "")
        //            .Trim();

        //        if (!Enum.TryParse<TicketCategory>(
        //                normalizedCategory,
        //                true,
        //                out var parsedCategory))
        //        {
        //            throw new ArgumentException(
        //                $"Invalid ticket category: '{category}'");
        //        }

        //        if (!Enum.TryParse<SentimentType>(
        //                sentiment,
        //                true,
        //                out var parsedSentiment))
        //        {
        //            throw new ArgumentException(
        //                $"Invalid sentiment: '{sentiment}'");
        //        }

        //        LatestReport = new TriageReport(
        //            parsedCategory, urgency, parsedSentiment);
        //    }

    }
}
