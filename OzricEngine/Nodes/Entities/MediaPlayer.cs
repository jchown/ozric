using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.MediaPlayer)]
public class MediaPlayer : EntityNode
{
    public override NodeType nodeType => NodeType.MediaPlayer;

    public const string OutputOn = "on";
    public const string OutputState = "state";

    public MediaPlayer(string id, string entityID) : base(id, entityID, null, new List<Pin> { new(OutputOn, ValueType.Binary), new(OutputState, ValueType.Mode)  })
    {
    }

    public override Task OnInit(Context context)
    {
        UpdateState(context);
        return Task.CompletedTask;
    }

    public override Task OnUpdate(Context context)
    {
        UpdateState(context);
        return Task.CompletedTask;
    }

    private void UpdateState(Context context)
    {
        var device = context.home.GetEntityState(entityID);
        if (device == null)
        {
            SetAlert(context, $"Unknown entity {entityID}");
            return;
        }
        
        var state = device.state;
        var on = new Binary(state != "unavailable" && state != "off");

        Log(LogLevel.Debug, "State = {1}, on = {0}", on, state);
        SetOutputValue(OutputState, new Mode(state), context);
        SetOutputValue(OutputOn, on, context);
    }
}