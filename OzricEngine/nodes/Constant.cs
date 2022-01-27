using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    [TypeKey(NodeType.Constant)]
    public class Constant: Node
    {
        public override NodeType nodeType => NodeType.Constant;

        public Value value { get; }

        public Constant(string id, Value value) : base(id, null, new List<Pin> { new("value", value.ValueType, value) })
        {
            this.value = value;
        }

        public override Task OnInit(Context context)
        {
            SetOutputValue("value", value);
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Context context)
        {
            SetOutputValue("value", value);
            return Task.CompletedTask;
        }
    }
}