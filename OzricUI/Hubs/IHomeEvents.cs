using Microsoft.AspNetCore.SignalR;
using OzricEngine;
using OzricEngine.Nodes;
using OzricEngine.Values;

namespace OzricUI.Hubs;

public interface IHomeEvents
{
    [HubMethodName(HomeHub.TEST_MESSAGE)]
    public Task Test(Value value);

    [HubMethodName(HomeHub.HEARTBEAT_MESSAGE)]
    public Task Heartbeat(string message);

    [HubMethodName(HomeHub.ENTITY_STATE_CHANGED_MESSAGE)]
    public Task EntityStateChanged(EventStateChanged esc);
    
    [HubMethodName(HomeHub.PIN_CHANGED_MESSAGE)]
    public Task PinChanged(string nodeID, string pinName, Value value);
}