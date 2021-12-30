using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class Constant<T>: Node where T: class
    {
        public T value { get; set; }
        
        public Constant(string id, T value) : base(id, null, new List<Pin> { new Pin("value", value) })
        {
            this.value = value;
        }

        public override void OnInit(Engine engine)
        {
            SetOutputValue("value", value);
        }

        public override void OnUpdate(Engine engine)
        {
            SetOutputValue("value", value);
        }
    }
}