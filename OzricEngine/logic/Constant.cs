using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    public class Constant<T>: Node where T: Value
    {
        public T value { get; }
        
        public Constant(string id, T value) : base(id, null, new List<Pin> { new Pin("value", value) })
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