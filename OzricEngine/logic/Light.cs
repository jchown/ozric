using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class Light: Node
    {
        public Light(string id) : base(id, new List<Input> { new Input("on-off", new OnOff()), new Input("colour", new Colour()) })
        {
        }

        public override void OnInit(Home home)
        {
        }

        public override void OnUpdate(Home home)
        {
        }
    }
}