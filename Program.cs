
using Azure;
using Azure.AI.OpenAI;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorage("chat_memory");
    siloBuilder.ConfigureLogging(x => x.ClearProviders());
});


var client = new OpenAIClient(
    new Uri(Environment.GetEnvironmentVariable("OPEN_AI_ENDPOINT") ?? ""),
    new AzureKeyCredential(Environment.GetEnvironmentVariable("OPEN_AI_KEY") ?? ""));

builder.Services.AddSingleton(client);

using WebApplication app = builder.Build();


app.MapGet("/", async (HttpRequest request, IGrainFactory grains) =>
    {
        var grain = grains.GetGrain<IChatGrain>("_");
        return await grain.GetTodos();
    });

app.MapPost("/",
    async (HttpRequest request, IGrainFactory grains) =>
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();

        var grain = grains.GetGrain<IChatGrain>("_");
        return await grain.Chat(body);
    });

Console.WriteLine("Ready...");

app.Run();


