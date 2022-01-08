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
                states.Add(state.entity_id, state);
        }

        public EntityState GetEntityState(string entityID)
        {
            return states[entityID] ?? throw new Exception("Unknown entity");
        }

        public virtual DateTime GetTime()
        {
            return DateTime.Now;
        }
    }
}