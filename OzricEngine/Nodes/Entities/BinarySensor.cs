using System.Collections.Generic;
using System.Threading.Tasks;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.Sensor)]
public class BinarySensor : EntityNode
{
    public override NodeType nodeType => NodeType.Sensor;

    public const string OUTPUT_NAME = "activity";

    public BinarySensor(string id, string entityID) : base(id, entityID, null, new List<Pin> { new(OUTPUT_NAME, ValueType.Binary) })
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

        SetOutputValue(OUTPUT_NAME, new Binary(device.state != "off"), context);
    }
}