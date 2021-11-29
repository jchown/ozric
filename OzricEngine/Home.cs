using System;
using System.Collections.Generic;

namespace OzricEngine.logic
{
    public class Home
    {
        private Dictionary<string, State> states;

        public Home(List<State> stateList)
        {
            states = new Dictionary<string, State>();
            foreach (var state in stateList)
                states[state.entity_id] = state;
        }

        public State Get(string entityID)
        {
            return states[entityID] ?? throw new Exception("Unknown entity");
        }

        public virtual DateTime GetTime()
        {
            return DateTime.Now;
        }
    }
}