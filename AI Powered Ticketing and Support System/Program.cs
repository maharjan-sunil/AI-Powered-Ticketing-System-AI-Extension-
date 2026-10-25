using AI_Powered_Ticketing_and_Support_System.Feature.TriageAutomation;
using AI_Powered_Ticketing_and_Support_System.Service;
using Google.GenAI;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.AI;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using static Microsoft.KernelMemory.OpenAIConfig;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

// Default to Gemini and your free Google Key
string model = config["ModelName"] ?? "gemini-2.5-flash";
string googleApiKey = config["GoogleKey"] ?? "";



// TEMPORARY TEST: Verify Gemini text generation
//var client = new ChatClient(
//    model: "gemini-3.8-flash",
//    credential: new ApiKeyCredential(googleApiKey),
//    options: new OpenAIClientOptions
//    {
//        Endpoint = new Uri(
//            "https://generativelanguage.googleapis.com/v1beta/openai/")
//    });

//var response = await client.CompleteChatAsync(
//    new UserChatMessage(
//        "Explain how to reset a customer password."));

//Console.WriteLine(response.Value.Content[0].Text);




// This maps Gemini to the IChatClient interface required by your TicketAutomationService
builder.Services.AddSingleton<IChatClient>(sp =>
{
    var client = new Google.GenAI.Client(
        apiKey: googleApiKey);

    return client.AsIChatClient(model);
});



var endpoint =
    "https://generativelanguage.googleapis.com/v1beta/openai/";

// Configure Gemini text generation through its OpenAI-compatible API.
var openAiConfig = new OpenAIConfig
{
    Endpoint = endpoint,
    APIKey = googleApiKey,
    TextModel = "gemini-2.5-flash",
    TextGenerationType = TextGenerationTypes.Chat,
    TextModelMaxTokenTotal = 8192,

    
};

// Build Kernel Memory.
var memory = new KernelMemoryBuilder()
    .WithCustomTextGenerator(
        new GeminiTextGenerator(googleApiKey))
    .WithCustomEmbeddingGenerator(
        new GeminiEmbeddingGenerator(googleApiKey))
    .WithCustomTextPartitioningOptions(
        new TextPartitioningOptions
        {
            MaxTokensPerParagraph = 300,
            OverlappingTokens = 30
        })
    .WithSimpleFileStorage(
        new SimpleFileStorageConfig
        {
            Directory = "km-files",
            StorageType = FileSystemTypes.Disk
        })
    .WithSimpleVectorDb(
        new SimpleVectorDbConfig
        {
            Directory = "km-vectors",
            StorageType = FileSystemTypes.Disk
        })
    .Build<MemoryServerless>();

//Inject IKernelMemory into controllers
builder.Services.AddSingleton<IKernelMemory>(memory);

// Inject for Traige Automation Service
builder.Services.AddScoped<TicketAutomationService>();

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