namespace OzricEngine
{
    /// <summary>
    /// ClientCommand deserializer
    /// </summary>
    public class JsonConverterClientCommand: JsonConverterBase<ClientCommand>
    {
        public JsonConverterClientCommand() : base("type")
        {
        }
    }
}