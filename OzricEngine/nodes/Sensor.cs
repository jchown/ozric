using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    public class Sensor : Node
    {
        private readonly string entityID;

        public Sensor(string id, string entityID) : base(id, null, new List<Pin> { new Pin("activity", ValueType.OnOff) })
        {
            this.entityID = entityID;
        }

        public override Task OnInit(Engine engine)
        {
            UpdateState(engine);
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Engine engine)
        {
            UpdateState(engine);
            return Task.CompletedTask;
        }

        private void UpdateState(Engine engine)
        {
            var device = engine.home.Get(entityID) ?? throw new Exception($"Unknown device {entityID}");
            var value = new OnOff(device.state != "off");

            Log(LogLevel.Debug, "activity = {0}", value);
            SetOutputValue("activity", value);
        }
    }
}