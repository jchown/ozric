using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class Light: Node
    {
        public Light(string id) : base(id, new List<Pin> { new Pin("on-off", new OnOff()), new Pin("colour", new Colour()) }, null)
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