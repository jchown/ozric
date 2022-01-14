using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    [TypeKey(NodeType.Sensor)]
    public class Sensor : Node
    {
        public override NodeType nodeType => NodeType.Sensor;

        private readonly string entityID;

        public Sensor(string id, string entityID) : base(id, null, new List<Pin> { new Pin("activity", ValueType.OnOff) })
        {
            this.entityID = entityID;
        }

        public override Task OnInit(Context context)
        {
            UpdateState(context);
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Context context)
        {
            UpdateState(context);
            return Task.CompletedTask;
        }

        private void UpdateState(Context context)
        {
            var engine = context.engine;
            var device = engine.home.GetEntityState(entityID) ?? throw new Exception($"Unknown device {entityID}");
            var value = new OnOff(device.state != "off");

            Log(LogLevel.Debug, "activity = {0}", value);
            SetOutputValue("activity", value);
        }
    }
}