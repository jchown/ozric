using OzricEngine;
using OzricService;
using OzricService.Model;

const int HOME_ASSISTANT_INGRESS_PORT = 8099;

var builder = WebApplication.CreateBuilder(args);

var service = new Service();
await service.Start(CancellationToken.None);

builder.WebHost.ConfigureKestrel(kestrelOptions => kestrelOptions.ListenAnyIP(HOME_ASSISTANT_INGRESS_PORT));

var app = builder.Build();

app.MapGet("/options", () => Options.Instance);
app.MapGet("/status", () => service.Status);
app.MapGet("/graph", () => service.Graph);
app.MapPut("/graph", (Graph graph) => service.Restart(graph));

app.Run();

