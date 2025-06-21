using System.Collections.Generic;
using System.Threading.Tasks;
using OzricEngine;
using OzricEngine.Nodes;
using OzricEngine.Values;
using ValueType = Ozric.Engine.Values.ValueType;

namespace Ozric.Engine.Graph.Logic;

/// <summary>
/// Choose from two values based on the state of an OnOff input
/// </summary>
[TypeKey(NodeType.BinaryChoice)]
public class BinaryChoice: Node
{
    public override NodeType nodeType => NodeType.BinaryChoice;

    public ValueType valueType;
        
    public const string InputNameOn = "on";
    public const string InputNameOff = "off";
    public const string InputNameSwitch = "switch";
    public const string OutputName = "output";
        
    public BinaryChoice(string id, ValueType valueType): base(id, new List<Pin> { new(InputNameOn, valueType), new(InputNameOff, valueType), new(InputNameSwitch, ValueType.Binary) }, new List<Pin> { new(OutputName, valueType) })
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