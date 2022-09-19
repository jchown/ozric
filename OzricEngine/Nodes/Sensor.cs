using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ValueType = OzricEngine.Values.ValueType;
using Boolean = OzricEngine.Values.Boolean;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.Sensor)]
public class Sensor : EntityNode
{
    public override NodeType nodeType => NodeType.Sensor;

    public const string OUTPUT_NAME = "activity";

    public Sensor(string id, string entityID) : base(id, entityID, null, new List<Pin> { new(OUTPUT_NAME, ValueType.Boolean) })
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
        var device = engine.home.GetEntityState(entityID);
        if (device == null)
        {
            Log(LogLevel.Warning, $"Unknown device {entityID}");
            return;
        }
        
        var value = new Boolean(device.state != "off");

        Log(LogLevel.Debug, "activity = {0}", value);
        SetOutputValue(OUTPUT_NAME, value);
    }
}