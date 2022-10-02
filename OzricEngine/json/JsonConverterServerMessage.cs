using System;

namespace OzricEngine
{
    /// <summary>
    /// ServerMessage deserializer
    /// </summary>
    public class JsonConverterServerMessage: JsonConverterBase<ServerMessage?>
    {
        public JsonConverterServerMessage() : base("type")
        {
        }

        protected override ServerMessage? OnUnrecognisedType(string name)
        {
            Console.WriteLine($"Ignoring unrecognised message {name}");
            return null;
        }
    }
}