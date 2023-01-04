using System;
using System.Text.Json;

namespace OzricEngine
{
    /// <summary>
    /// Event deserializer
    /// </summary>
    public class JsonConverterEvent: JsonConverterBase<Event>
    {
        public JsonConverterEvent() : base("event_type")
        {
        }
        
        protected override Event OnUnrecognisedType(JsonDocument doc, string name)
        {
            Console.WriteLine($"Unknown event_type \"{name}\" for {nameof(Event)}");
            return doc.Deserialize<EventUnknown>();
        }
    }
}