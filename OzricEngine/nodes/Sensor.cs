using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    [TypeKey(NodeType.Sensor)]
    public class Sensor : Node
    {
        public override NodeType nodeType => NodeType.Sensor;

        public string entityID { get; }

        public const string OUTPUT_NAME = "activity";

        public Sensor(string id, string entityID) : base(id, null, new List<Pin> { new(OUTPUT_NAME, ValueType.Boolean) })
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
            var value = new Boolean(device.state != "off");

            Log(LogLevel.Debug, "activity = {0}", value);
            SetOutputValue("activity", value);
        }

        #region Comparison
        public override bool Equals(object obj)
        {
            if (!(obj is Sensor sensor))
                return false;
        
            return base.Equals(obj) && entityID == sensor.entityID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GetHashCode(), entityID);
        }
        #endregion
    }
}