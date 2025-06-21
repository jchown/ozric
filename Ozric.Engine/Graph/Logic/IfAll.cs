using System.Collections.Generic;
using System.Threading.Tasks;
using OzricEngine;
using OzricEngine.Nodes;
using OzricEngine.Values;
using ValueType = Ozric.Engine.Values.ValueType;

namespace Ozric.Engine.Graph.Logic;

[TypeKey(NodeType.IfAll)]
public class IfAll: VariableInputs
{
    public static readonly string OutputName = "output";
        
    public override NodeType nodeType => NodeType.IfAll;

    public IfAll(): this("")
    {
    }

    public IfAll(string id): base(id, ValueType.Binary, new List<Pin> { new(OutputName, ValueType.Binary) })
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
        SetOutputValue(OutputName, value, context);
    }
}