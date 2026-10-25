using Google.GenAI;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
namespace AI_Powered_Ticketing_and_Support_System.Service
{
    public sealed class GeminiEmbeddingGenerator : ITextEmbeddingGenerator
    {
        private readonly Client _client;

        public GeminiEmbeddingGenerator(string apiKey)
        {
            _client = new Client(apiKey: apiKey);
        }

        public int MaxTokens => 2048;

        public int CountTokens(string text)
        {
            // Approximate count for partitioning.
            // This is not Gemini's exact tokenizer.
            return GetTokens(text).Count;
        }

        public IReadOnlyList<string> GetTokens(string text)
        {
            return text.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries);
        }

         async Task<Embedding> ITextEmbeddingGenerator.GenerateEmbeddingAsync(
            string text,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = await _client.Models.EmbedContentAsync(
                model: "gemini-embedding-001",
                contents: text);

            var values = response.Embeddings?
                .FirstOrDefault()?
                .Values;

            if (values == null || values.Count == 0)
            {
                throw new InvalidOperationException(
                    "Gemini returned an empty embedding.");
            }

            var vector = values
                .Select(value => Convert.ToSingle(value))
                .ToArray();

            return new Embedding(vector);
        }
    }
}

