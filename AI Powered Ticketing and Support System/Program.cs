using Microsoft.AspNetCore.Http.Features;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using static Microsoft.KernelMemory.OpenAIConfig;

var builder = WebApplication.CreateBuilder(args);

// ---------- Config (falls back to local Ollama defaults) ----------
var aiConfig = builder.Configuration.GetSection("AI");
var endpoint = aiConfig["Endpoint"] ?? "http://localhost:11434/v1";
var chatModel = aiConfig["ChatModel"] ?? "llama3.1:8b";
var embeddingModel = aiConfig["EmbeddingModel"] ?? "bge-large";

// ---------- Kernel Memory using Ollama's OpenAI-compatible API ----------
var openAiConfig = new OpenAIConfig
{
    Endpoint = endpoint,
    APIKey = "local-sandbox",                    // Ollama ignores it, but it must be non-empty
    TextModel = chatModel,
    TextGenerationType = TextGenerationTypes.Chat,
    TextModelMaxTokenTotal = 8192,
    EmbeddingModel = embeddingModel,
    EmbeddingModelMaxTokenTotal = 512
};

var memory = new KernelMemoryBuilder()
    .WithOpenAITextGeneration(openAiConfig)
    .WithOpenAITextEmbeddingGeneration(openAiConfig)
    .WithCustomTextPartitioningOptions(new TextPartitioningOptions
    {
        MaxTokensPerParagraph = 300,
        OverlappingTokens = 30
    })
    .WithSimpleFileStorage(new SimpleFileStorageConfig
    {
        Directory = "km-files",
        StorageType = FileSystemTypes.Disk
    })
    .WithSimpleVectorDb(new SimpleVectorDbConfig
    {
        Directory = "km-vectors",
        StorageType = FileSystemTypes.Disk
    })
    .Build<MemoryServerless>();

// Inject IKernelMemory into controllers
builder.Services.AddSingleton<IKernelMemory>(memory);

// Allow uploads up to 50 MB
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 50_000_000);
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 50_000_000);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();