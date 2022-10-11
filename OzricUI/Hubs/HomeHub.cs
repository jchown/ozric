using Microsoft.AspNetCore.SignalR;

namespace OzricUI.Hubs;

/// <summary>
/// SignalR channel to send updates to the browser
/// </summary>
public class HomeHub: Hub<IHomeEvents>
{
    public const string ENDPOINT = "/home/hub";
    public const string HEARTBEAT_MESSAGE = "Heartbeat";
    public const string ENTITY_STATE_CHANGED_MESSAGE = "EntityStateChanged";
    public const string PIN_CHANGED_MESSAGE = "PinChanged";

    //  TODO: Secure this!

    public HomeHub()
    {
    }

}