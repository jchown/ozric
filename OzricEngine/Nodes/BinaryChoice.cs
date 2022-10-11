using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Boolean = OzricEngine.Values.Boolean;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

/// <summary>
/// Choose from two values based on the state of an OnOff input
/// </summary>
[TypeKey(NodeType.BooleanChoice)]
public class BinaryChoice: Node
{
    public override NodeType nodeType => NodeType.BooleanChoice;

    public ValueType valueType { get; set; }
        
    public const string INPUT_NAME_ON = "on";
    public const string INPUT_NAME_OFF = "off";
    public const string INPUT_NAME_SWITCH = "switch";
    public const string OUTPUT_NAME = "output";
        
    public BinaryChoice(string id, ValueType valueType): base(id, new List<Pin> { new(INPUT_NAME_ON, valueType), new(INPUT_NAME_OFF, valueType), new(INPUT_NAME_SWITCH, ValueType.Boolean) }, new List<Pin> { new(OUTPUT_NAME, valueType) })
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
        var switcher = GetInputValue<Boolean>(INPUT_NAME_SWITCH);
        var input = (switcher.value) ? GetInput(INPUT_NAME_ON) : GetInput(INPUT_NAME_OFF);
        SetOutputValue(OUTPUT_NAME, input.value!, context);
    }
}