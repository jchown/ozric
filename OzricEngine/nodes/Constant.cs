using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    [TypeKey(NodeType.Constant)]
    public class Constant: Node
    {
        public override NodeType nodeType => NodeType.Constant;

        public Value value { get; set; }

        public Constant()
        {
        }

        public Constant(string id, ValueType type, Value value) : base(id, null, new List<Pin> { new Pin("value", type, value) })
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