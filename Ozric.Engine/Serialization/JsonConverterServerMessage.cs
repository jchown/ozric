using System;
using System.Text.Json;
using Sentry;

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

        protected override ServerMessage? OnUnrecognisedType(JsonDocument doc, string name)
        {
            Console.WriteLine($"Ignoring unrecognised message {name}");
            SentrySdk.CaptureMessage($"Ignoring unrecognised message {name}: {doc}");
                
            return null;
        }
    }
}