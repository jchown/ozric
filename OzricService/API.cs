using OzricEngine;
using OzricEngine.engine;
using OzricService.Model;

namespace OzricService;

public class API
{
    public static void Map(WebApplication app)
    {
        var engineService = app.Services.GetService<IEngineService>() ?? throw new InvalidOperationException("No IEngineService");
        
        app.MapGet("/api/options", () => Options.Instance);
        app.MapGet("/api/status", () => engineService.Status);
        app.MapPut("/api/status", (EngineStatus status) => engineService.SetPaused(status.paused));
        app.MapGet("/api/graph", () => engineService.Graph);
        app.MapPut("/api/graph", async (Graph graph) => await engineService.Restart(graph));
        app.MapGet("/api/home", () => engineService.Home);
        app.MapGet("/api/headers", (HttpRequest request) => request.Headers);
    }
}