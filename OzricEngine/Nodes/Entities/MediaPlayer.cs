using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OzricEngine.Values;
using Boolean = OzricEngine.Values.Boolean;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.MediaPlayer)]
public class MediaPlayer : EntityNode
{
    public override NodeType nodeType => NodeType.MediaPlayer;

    public const string ON_NAME = "on";

    public MediaPlayer(string id, string entityID) : base(id, entityID, null, new List<Pin> { new(ON_NAME, ValueType.Boolean) })
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
        var engine = context.engine;
        var device = engine.home.GetEntityState(entityID) ?? throw new Exception($"Unknown device {entityID}");
        var value = new Boolean(device.state != "off");

        Log(LogLevel.Debug, "on = {0}", value);
        SetOutputValue(ON_NAME, value);
    }
}