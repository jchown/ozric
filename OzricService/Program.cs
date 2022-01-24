using OzricEngine;
using OzricService;
using OzricService.Model;

const int HOME_ASSISTANT_INGRESS_PORT = 8099;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.WebHost.ConfigureKestrel(kestrelOptions => kestrelOptions.ListenAnyIP(HOME_ASSISTANT_INGRESS_PORT));

await Service.Instance.Start(CancellationToken.None);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");


app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.MapGet("/engine/options", () => Options.Instance);
app.MapGet("/engine/status", () => Service.Instance.Status);
app.MapGet("/engine/graph", () => Service.Instance.Graph);
app.MapPut("/engine/graph", (Graph graph) => Service.Instance.Restart(graph));

app.Run();
