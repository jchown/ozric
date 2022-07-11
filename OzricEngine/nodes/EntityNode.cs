using System;
using System.Collections.Generic;

namespace OzricEngine.logic
{
    /// <summary>
    /// A node that maps to a HomeAssistant entity.
    /// </summary>
    public abstract class EntityNode: Node
    {
        public string entityID { get; }

        protected EntityNode(string id, string entityID, List<Pin> inputs, List<Pin> outputs): base(id, inputs, outputs)
        {
            this.entityID = entityID;
        }

        protected float GetSecondsSinceLastUpdated(Engine engine)
        {
            var state = engine.home.GetEntityState(entityID);
            if (state == null)
                return float.MaxValue;

            return (float)(engine.home.GetTime() - state.last_updated).TotalSeconds;
        }

        public static Type? ForDevice(EntityState entityState)
        {
            if (entityState.entity_id.StartsWith("light-"))
            {
                return typeof(Light);
            }

            return null;
        }
    }
}