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

        public override void OnInit(Home home)
        {
            UpdateState(home);
        }

        public override void OnUpdate(Home home)
        {
            UpdateState(home);
        }

        private void UpdateState(Home home)
        {
            var device = home.Get(entityID) ?? throw new Exception($"Unknown device {entityID}");
            var value = new OnOff(device.state != "off");

            home.Log($"{id}.activity = {value}");
            SetOutputValue("activity", value);
        }
    }
}