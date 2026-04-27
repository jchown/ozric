using System.Collections.Generic;
using Ozric.Engine.Model;

namespace Ozric.Engine
{
    [TypeKey("get_states")]
    public class ServerGetStates: ServerResult
    {
        public List<EntityState> result { get; set; }
    }
}