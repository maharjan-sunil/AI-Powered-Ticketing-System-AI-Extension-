
using Google.GenAI;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;

namespace AI_Powered_Ticketing_and_Support_System.Service
{
    public sealed class GeminiTextGenerator : ITextGenerator
    {

        private readonly Client _client;

        public GeminiTextGenerator(string apiKey)
        {
            _client = new Client(apiKey: apiKey);
        }

        public int MaxTokenTotal => 8192;

        // Approximate word count, not an exact Gemini token count.
        public int CountTokens(string text)
        {
            return string.IsNullOrWhiteSpace(text)
                ? 0
                : text.Split(
                    (char[]?)null,
                    StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public IReadOnlyList<string> GetTokens(string text)
        {
            return text.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries);
        }

        public async IAsyncEnumerable<GeneratedTextContent> GenerateTextAsync(
            string prompt,
            TextGenerationOptions options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = await _client.Models.GenerateContentAsync(
                model: "gemini-3.8-flash",
                contents: prompt);

            cancellationToken.ThrowIfCancellationRequested();

            var generatedText =
                response.Candidates?[0].Content?.Parts?[0].Text;

            yield return new GeneratedTextContent(
                generatedText ?? string.Empty);
        }
    }
}