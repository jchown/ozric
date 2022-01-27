using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.FileProviders;
using OzricEngine;
using OzricService;
using OzricService.Model;

const int homeAssistantIngressPort = 8099;
const string dockerWwwRoot = "/ozric/wwwroot";

var builder = WebApplication.CreateBuilder(args);

StaticFileOptions? staticFileOptions;
    
Console.WriteLine($"Web root: {builder.Environment.WebRootPath}");
Console.WriteLine($"Content root: {builder.Environment.ContentRootPath}");

if (Directory.Exists(dockerWwwRoot))
{
    Console.WriteLine($"Static files: {dockerWwwRoot}");

    staticFileOptions = new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(dockerWwwRoot),
        RequestPath = ""
    };
}
else
{
    Console.WriteLine("Static files: default");

    staticFileOptions = new StaticFileOptions();
}

builder.WebHost.ConfigureKestrel(kestrelOptions => kestrelOptions.ListenAnyIP(homeAssistantIngressPort));
builder.Services.Configure<JsonOptions>(options => Json.Configure(options.SerializerOptions));

await Service.Instance.Start(CancellationToken.None);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles(staticFileOptions);
app.UseRouting();

app.MapGet("/api/options", () => Options.Instance);
app.MapGet("/api/status", () => Service.Instance.Status);
app.MapGet("/api/graph", () => Service.Instance.Graph);
app.MapPut("/api/graph", async (Graph graph) => await Service.Instance.Restart(graph));

app.Run();
