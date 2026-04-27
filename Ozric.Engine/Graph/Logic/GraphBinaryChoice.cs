using System.Collections.Generic;
using System.Threading.Tasks;
using Ozric.Engine.Nodes;
using Ozric.Engine.Values;
using ValueType = Ozric.Engine.Values.ValueType;

namespace Ozric.Engine.Graph.Logic;

/// <summary>
/// Choose from two values based on the state of an OnOff input
/// </summary>
[TypeKey(NodeType.BinaryChoice)]
public class GraphBinaryChoice: GraphNode
{
    public override NodeType nodeType => NodeType.BinaryChoice;

    public ValueType valueType;
        
    public const string InputNameOn = "on";
    public const string InputNameOff = "off";
    public const string InputNameSwitch = "switch";
    public const string OutputName = "output";
        
    public GraphBinaryChoice(string id, ValueType valueType): base(id, new List<Pin> { new(InputNameOn, valueType), new(InputNameOff, valueType), new(InputNameSwitch, ValueType.Binary) }, new List<Pin> { new(OutputName, valueType) })
    {
        this.valueType = valueType;
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
        var switcher = GetInputValue<Binary>(InputNameSwitch);
        var input = (switcher.value) ? GetInput(InputNameOn) : GetInput(InputNameOff);
        SetOutputValue(OutputName, input.value!, context);
    }
}