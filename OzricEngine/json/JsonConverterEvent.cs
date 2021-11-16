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
    }
}