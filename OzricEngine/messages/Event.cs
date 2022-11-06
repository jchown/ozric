using System;
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
        public string event_type;
        public string origin;
        public DateTime time_fired;
        public MessageContext context;

        public override string ToString()
        {
            return Json.Serialize(this);
        }
    }
}