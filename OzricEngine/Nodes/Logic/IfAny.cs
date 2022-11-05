using System.Collections.Generic;
using System.Threading.Tasks;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.IfAny)]
public class IfAny: VariableInputs
{
    public const string OUTPUT_NAME = "output";
        
    public override NodeType nodeType => NodeType.IfAny;

    public IfAny() : this(null)
    {
    }

    public IfAny(string id): base(id, ValueType.Binary, new List<Pin> { new(OUTPUT_NAME, ValueType.Binary) })
    {
    }

    public IfAny(string id, params string[] inputNames): this(id)
    {
        foreach (var inputName in inputNames)
            AddInput(inputName);
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
        var on = false;
        foreach (var onOff in GetInputValues<Binary>())
            on |= onOff?.value ?? false;

        var value = new Binary(on);
        SetOutputValue(OUTPUT_NAME, value, context);
    }
}