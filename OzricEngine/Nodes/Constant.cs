using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OzricEngine.Values;

namespace OzricEngine.logic
{
    [TypeKey(NodeType.Constant)]
    public class Constant: Node
    {
        public const string OUTPUT_NAME = "value";
        
        public override NodeType nodeType => NodeType.Constant;

        public Value value { get; set; }

        public Constant(string id, Value value) : base(id, null, new List<Pin> { new(OUTPUT_NAME, value.ValueType, value) })
        {
            this.value = value;
        }

        public override Task OnInit(Context context)
        {
            SetOutputValue(OUTPUT_NAME, value);
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Context context)
        {
            SetOutputValue(OUTPUT_NAME, value);
            return Task.CompletedTask;
        }
    }
}