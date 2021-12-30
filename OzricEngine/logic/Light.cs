using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class Light: Node
    {
        private readonly string entityID;

        public Light(string id, string entityID) : base(id, new List<Pin> { new Pin("on-off", new OnOff()), new Pin("colour", new Colour()) }, null)
        {
            this.entityID = entityID;
        }

        public override void OnInit(Engine engine)
        {
            UpdateValue(engine);
        }

        public override void OnUpdate(Engine engine)
        {
            UpdateValue(engine);
        }

        private void UpdateValue(Engine engine)
        {
            //engine.
        }
    }
}