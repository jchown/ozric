using System.Collections.Generic;

namespace OzricEngine
{
    public class ServerGetStates: ServerMessage
    {
        public int id { get; set; }
        public bool success { get; set; }
        public string error { get; set; }
        public List<State> result { get; set; }
    }
}