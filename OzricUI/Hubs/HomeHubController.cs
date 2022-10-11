using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OzricEngine.Nodes;
using OzricEngine.Values;
using OzricService;

namespace OzricUI.Hubs;

public class HomeHubController: IHomeHubController
{
    private readonly IHubContext<HomeHub, IHomeEvents> _hubContext;
    private readonly ILogger<HomeHubController> _logger;

    public HomeHubController(IEngineService engine, IHubContext<HomeHub, IHomeEvents> hubContext, ILogger<HomeHubController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
        _logger.Log(LogLevel.Information, "HomeHubController started");

        var engineService = ((EngineService) engine);
        engineService.Engine.entityStateChanged += esc => Task.Run(async () =>
        {
            await _hubContext.Clients.All.EntityStateChanged(esc);
        });
        
        engineService.Engine.pinChanged += (nodeID, pinName, value) => Task.Run(async () =>
        {
            await _hubContext.Clients.All.PinChanged(nodeID, pinName, value);
        });

        var heartbeat = new System.Timers.Timer();
        heartbeat.Interval = 3000;
        heartbeat.Elapsed += (_, _) => Task.Run(() => _hubContext.Clients.All.Heartbeat("❤️"));
        heartbeat.Enabled = true;
    }
}
