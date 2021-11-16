namespace OzricEngine
{
    /// <summary>
    /// ServerMessage deserializer
    /// </summary>
    public class JsonConverterServerMessage: JsonConverterBase<ServerMessage>
    {
        public JsonConverterServerMessage() : base("type")
        {
        }
    }
}