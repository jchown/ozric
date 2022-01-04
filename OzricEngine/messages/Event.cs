using JetBrains.Annotations;
using System.Text.Json;

namespace OzricEngine
{
    /// <summary>
    /// Base class for events, the payload of a ServerEvent, sent by the server. The "event_type" field indicates the specific type.  
    /// </summary>
    /// <see cref="ServerEvent"/>
    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class Event
    {
        public string event_type { get; set; }
        
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}