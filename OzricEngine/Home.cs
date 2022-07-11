using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OzricEngine.logic
{
    public class Home
    {
        public Dictionary<string, EntityState> states { get; set; } = new();

        public Home()
        {
        }

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

        public List<EntityState> GetEntityStates(List<string> entityIDs)
        {
            var selected = states.Values.Where(es => entityIDs.Contains(es.entity_id)).ToList();
            selected.Sort((e1, e2) => string.CompareOrdinal(e1.entity_id, e2.entity_id));
            return selected;
        }
    }
}