using Microsoft.AspNetCore.Http.Json;
using OzricEngine;
using OzricService;
using OzricService.Model;

const int homeAssistantIngressPort = 8099;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(kestrelOptions => kestrelOptions.ListenAnyIP(homeAssistantIngressPort));
builder.Services.Configure<JsonOptions>(options => Json.Configure(options.SerializerOptions));

await Service.Instance.Start(CancellationToken.None);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapGet("/api/options", () => Options.Instance);
app.MapGet("/api/status", () => Service.Instance.Status);
app.MapGet("/api/graph", () => Service.Instance.Graph);
app.MapPut("/api/graph", async (Graph graph) => await Service.Instance.Restart(graph));

app.Run();
