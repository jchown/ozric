using OzricEngine;
using OzricEngine.engine;
using OzricService.Model;

namespace OzricService;

public class API
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/options", () => Options.Instance);
        app.MapGet("/api/status", () => Service.Instance.Status);
        app.MapPut("/api/status", (EngineStatus status) => Service.Instance.SetPaused(status.paused));
        app.MapGet("/api/graph", () => Service.Instance.Graph);
        app.MapPut("/api/graph", async (Graph graph) => await Service.Instance.Restart(graph));
        app.MapGet("/api/home", () => Service.Instance.Home);
    }
}