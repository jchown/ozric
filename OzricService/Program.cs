using OzricEngine;
using OzricService;
using OzricService.Model;

var builder = WebApplication.CreateBuilder(args);

var service = new EngineService();
await service.Start(CancellationToken.None);

var app = builder.Build();

app.MapGet("/options", () => Options.Instance);
app.MapGet("/status", () => service.Status);
app.MapGet("/graph", () => service.Graph);
app.MapPut("/graph", (Graph graph) => service.Restart(graph));

app.Run();

