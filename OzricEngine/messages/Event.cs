namespace OzricEngine
{
    /// <summary>
    /// Base class for events, the payload of a ServerMessageEvent, sent by the server. The "event_type" field indicates the specific type.  
    /// </summary>
    /// <see cref="ServerMessageEvent"/>

    public abstract class Event
    {
        public string event_type { get; set; }
    }
}