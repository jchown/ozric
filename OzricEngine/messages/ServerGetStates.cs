using System.Collections.Generic;

namespace OzricEngine
{
    [TypeKey("get_states")]
    public class ServerGetStates: ServerMessage
    {
        public int id { get; set; }
        public bool success { get; set; }
        public string error { get; set; }
        public List<EntityState> result { get; set; }
    }
}