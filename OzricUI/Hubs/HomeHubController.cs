using Microsoft.AspNetCore.SignalR;
using OzricEngine;
using OzricEngine.Values;
using OzricService;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace OzricUI.Hubs;

public class HomeHubController: IHomeHubController
{
    private readonly IHubContext<HomeHub, IHomeEvents> _hubContext;
    private readonly ILogger<HomeHubController> _logger;

    public HomeHubController(IEngineService engineService, IHubContext<HomeHub, IHomeEvents> hubContext, ILogger<HomeHubController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
        _logger.Log(LogLevel.Information, "HomeHubController started");

        // engineService.Engine.entityStateChanged += esc => Task.Run(async () =>
        // {
        //     await _hubContext.Clients.All.EntityStateChanged(esc);
        // });
        
        engineService.Subscribe(OnPinChanged);

        var heartbeat = new System.Timers.Timer();
        heartbeat.Interval = 3000;
        heartbeat.Elapsed += (_, _) => Task.Run(() => _hubContext.Clients.All.Heartbeat("❤️"));
        heartbeat.Enabled = true;
    }

    private void OnPinChanged(string nodeID, string pinName, Value value)
    {
        Task.Run(async () =>
        {
            await _hubContext.Clients.All.PinChanged(nodeID, pinName, Json.Serialize(value));
        });
    }
}
