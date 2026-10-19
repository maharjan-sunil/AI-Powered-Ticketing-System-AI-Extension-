using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.AI; // Native .NET AI Abstraction Layer

namespace AI_Powered_Ticketing_and_Support_System.Feature.TriageAutomation
{
    public class TicketAutomationService
    {
        private readonly IChatClient _chatClient;

        public TicketAutomationService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<TriageReport> ExtractTriageDetailsWithFunctionCallingAsync(string userMessage)
        {
            try
            {
                // 1. Initialize our local tool class instance
                var toolsInstance = new TicketTools();

                // 2. Wrap the tool instance methods so the AI can read its signatures
                var chatOptions = new ChatOptions
                {
                    Tools = [AIFunctionFactory.Create(toolsInstance.SubmitTriageReport)]
                };

                // ─── FIXED PIPELINE DEFINITION ───
                // Directly passing the _chatClient into the constructor bypasses any missing .AsBuilder() extensions
                var pipeline = new ChatClientBuilder(_chatClient)
                    .UseFunctionInvocation() // Automatically invokes the method when Gemini requests it
                    .Build();

                // 3. Construct a standard conversational prompt
                var systemMessage = """
                                        You are a helpdesk ticket triage assistant.

                                        Analyze the user's ticket and call SubmitTriageReport.

                                        Use only these exact category values:
                                        - AccountAccess
                                        - Technical
                                        - Billing
                                        - Network

                                        Use only these exact sentiment values:
                                        - Positive
                                        - Neutral
                                        - Negative
                                        - Angry
                                        - Frustrated

                                        Do not return descriptions or alternative names.
                                        For example, a login issue must use AccountAccess.

                                        Urgency must be an integer from 1 to 5.
                                        You must call SubmitTriageReport.
                                        """;
                var userPrompt = $"User Ticket Message: \"{userMessage}\"";

                var chatMessages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, systemMessage),
                new ChatMessage(ChatRole.User, userPrompt)
            };

                try
                {
                    await pipeline.GetResponseAsync(chatMessages, chatOptions);
                    Console.WriteLine("Pipeline completed successfully");
                }
                catch (System.ClientModel.ClientResultException ex)
                {
                    var raw = ex.GetRawResponse();
                    Console.WriteLine(raw?.Content.ToString()); // OpenAI's JSON error with "message" and "param"
                    throw;
                }

                //// 5. Check if the AI populated our tool callback payload successfully
                if (toolsInstance.LatestReport != null)
                {
                    return toolsInstance.LatestReport;
                }

                // Fallback safety if the model tried to reply with text instead of running the tool
                throw new InvalidOperationException("The Gemini model failed to automatically invoke or populate the required tool layout.");
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new InvalidOperationException("An error occurred while extracting triage details.", ex);
            }
        }
    }
}
