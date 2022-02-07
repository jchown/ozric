using System;
using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class Home
    {
        private Dictionary<string, EntityState> states;

        public Home(List<EntityState> stateList)
        {
            states = new Dictionary<string, EntityState>();
            foreach (var state in stateList)
            {
                states.Add(state.entity_id, state);

                if (state.entity_id.StartsWith("light."))
                    state.LogLightState();
            }
        }

        public EntityState GetEntityState(string entityID)
        {
            return states.GetValueOrDefault(entityID);
        }

        public virtual DateTime GetTime()
        {
            return DateTime.Now;
        }
    }
}