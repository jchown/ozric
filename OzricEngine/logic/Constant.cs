using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class Constant<T>: Node where T: class
    {
        public T value;
        
        public Constant(string id, T value) : base(id, new List<Output> { new Output("value", value) })
        {
            this.value = value;
        }

        public override void OnInit(Home home)
        {
            SetOutputValue("value", value);
        }

        public override void OnUpdate(Home home)
        {
            SetOutputValue("value", value);
        }
    }
}