using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.IfAll)]
public class IfAll: VariableInputs
{
    public static string OUTPUT_NAME = "output";
        
    public override NodeType nodeType => NodeType.IfAll;

    public IfAll(string id): base(id, ValueType.Boolean, new List<Pin> { new(OUTPUT_NAME, ValueType.Boolean) })
    {
    }
            
    public override Task OnInit(Context context)
    {
        UpdateValue(context.engine);
        return Task.CompletedTask;
    }

    public override Task OnUpdate(Context context)
    {
        UpdateValue(context.engine);
        return Task.CompletedTask;
    }

    private void UpdateValue(Engine engine)
    {
        var on = true;
        foreach (var onOff in GetInputValues<Boolean>())
            on &= onOff.value;

        var value = new Boolean(on);
        Log(LogLevel.Debug, "output = {0}", value);
        SetOutputValue("output", value);
    }
}