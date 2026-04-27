using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Ozric.Engine.Graph;
using Ozric.Engine.Values;

namespace Ozric.Engine.Nodes;

[TypeKey(NodeType.Constant)]
public class GraphConstant: GraphNode
{
    public const string OutputName = "value";
        
    public override NodeType nodeType => NodeType.Constant;

    [JsonIgnore] public ValueType ConstantType => GetOutput(OutputName).type;

    [JsonIgnore] public Value? ConstantValue => GetOutput(OutputName).value;

    public Value value { get; set; }

    public GraphConstant(string id, Value value) : base(id, null, new List<Pin> { new(OutputName, value.ValueType, value) })
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