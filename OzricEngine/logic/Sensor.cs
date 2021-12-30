using System;
using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class Sensor : Node
    {
        private readonly string entityID;

        public Sensor(string id, string entityID) : base(id, null, new List<Pin> { new Pin("activity", new OnOff()) })
        {
            this.entityID = entityID;
        }

        public override void OnInit(Engine engine)
        {
            UpdateState(engine);
        }

        public override void OnUpdate(Engine engine)
        {
            UpdateState(engine);
        }

        private void UpdateState(Engine engine)
        {
            var device = engine.home.Get(entityID) ?? throw new Exception($"Unknown device {entityID}");
            var value = new OnOff(device.state != "off");

            engine.home.Log($"{id}.activity = {value}");
            SetOutputValue("activity", value);
        }
    }
}