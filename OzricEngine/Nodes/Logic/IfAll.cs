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

    public IfAll(string id): base(id, ValueType.Binary, new List<Pin> { new(OUTPUT_NAME, ValueType.Binary) })
    {
    }
            
    public override Task OnInit(Context context)
    {
        UpdateValue(context);
        return Task.CompletedTask;
    }

    public override Task OnUpdate(Context context)
    {
        UpdateValue(context);
        return Task.CompletedTask;
    }

    private void UpdateValue(Context context)
    {
        var on = true;
        foreach (var onOff in GetInputValues<Binary>())
            on &= onOff.value;

        var value = new Binary(on);
        SetOutputValue(OUTPUT_NAME, value, context);
    }
}