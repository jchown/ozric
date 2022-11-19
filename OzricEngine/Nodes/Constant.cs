using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OzricEngine.Values;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.Constant)]
public class Constant: Node
{
    public const string OutputName = "value";
        
    public override NodeType nodeType => NodeType.Constant;

    public Value value { get; set; }

    public Constant(string id, Value value) : base(id, null, new List<Pin> { new(OutputName, value.ValueType, value) })
    {
        this.value = value;
    }

    public override Task OnInit(Context context)
    {
        SetOutputValue(OutputName, value, context);
        return Task.CompletedTask;
    }

    public override Task OnUpdate(Context context)
    {
        SetOutputValue(OutputName, value, context);
        return Task.CompletedTask;
    }
}