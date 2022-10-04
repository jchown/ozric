using Microsoft.AspNetCore.SignalR;
using OzricEngine;
using OzricService;
using Timer = System.Timers.Timer;

namespace OzricUI.Hubs;

/// <summary>
/// SignalR channel to send updates to the browser
/// </summary>
public class HomeHub: Hub
{
    private readonly ILogger<HomeHub> logger;

    public const string ENDPOINT = "/home/hub";
    public const string HEARTBEAT_MESSAGE = "Heartbeat";
    public const string STATE_CHANGED_MESSAGE = "StateChanged";

    //  TODO: Secure this!

    public HomeHub(IEngineService engine, ILogger<HomeHub> logger)
    {
        this.logger = logger;

        ((EngineService) engine).Engine.stateChangeHandlers += (esc) => Task.Run(async () => await SendMessage(esc));

        var heartbeat = new Timer();
        heartbeat.Interval = 3000;
        heartbeat.Elapsed += (_, _) => Task.Run(() => SendHeartbeat());
        heartbeat.Enabled = true;
    }

    public async Task SendHeartbeat()
    {
        await Clients.All.SendAsync(HEARTBEAT_MESSAGE, "❤");
    }

    public async Task SendMessage(EventStateChanged esc)
    {
        await Clients.All.SendAsync(STATE_CHANGED_MESSAGE, esc);
    }
}