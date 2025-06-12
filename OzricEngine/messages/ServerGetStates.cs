using System.Collections.Generic;

namespace OzricEngine
{
    [TypeKey("get_states")]
    public class ServerGetStates: ServerResult
    {
        public List<EntityState> result { get; set; }
    }
}