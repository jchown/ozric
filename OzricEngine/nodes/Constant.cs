using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    public class Constant: Node
    {
        public Value value { get; }
        
        public Constant(string id, ValueType type, Value value) : base(id, null, new List<Pin> { new Pin("value", type, value) })
        {
            this.value = value;
        }

        public override Task OnInit(Engine engine)
        {
            SetOutputValue("value", value);
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Engine engine)
        {
            SetOutputValue("value", value);
            return Task.CompletedTask;
        }
    }
}