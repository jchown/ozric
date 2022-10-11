using System.Collections.Generic;
using System.Threading.Tasks;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.ModeSensor)]
public class ModeSensor : EntityNode
{
    public override NodeType nodeType => NodeType.ModeSensor;

    public const string OUTPUT_NAME = "state";

    public ModeSensor(string id, string entityID) : base(id, entityID, null, new List<Pin> { new(OUTPUT_NAME, ValueType.Mode) })
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
            Log(LogLevel.Warning, $"Unknown device {entityID}");
            return;
        }

        SetOutputValue(OUTPUT_NAME, new Mode(device.state), context);
    }
}