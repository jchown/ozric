using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine
{
    public class JsonConverters
    {
        private static readonly IDictionary<ServerResult.Type, Type> ResultTypes = new Dictionary<ServerResult.Type, Type>();

        static JsonConverters()
        {
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => typeof(ServerResult).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract))
            {
                var attr = type.GetCustomAttribute<ServerResultTypeAttribute>();
                ResultTypes[attr.value] = type;
            }
        }

        public class ResultConverter: JsonConverter<ServerResult> 
        {
            public override ServerResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                using (var jsonDocument = JsonDocument.ParseValue(ref reader))
                {
                    if (!jsonDocument.RootElement.TryGetProperty("type", out var typeProperty))
                    {
                        throw new JsonException();
                    }

                    var value = typeProperty.GetString();
                    if (!ServerResult.Type.TryParse(value, out ServerResult.Type enumType))
                    {
                        throw new JsonException($"Could not parse {value} as {typeof(ServerResult.Type)}");
                    }
                    
                    var type = ResultTypes[enumType];
                    if (type == null)
                    {
                        throw new JsonException($"Unknown class for {enumType} of {typeof(ServerResult.Type)}");
                    }

                    var jsonObject = jsonDocument.RootElement.GetRawText();
                    return (ServerResult) JsonSerializer.Deserialize(jsonObject, type, options);
                }
            }

            public override void Write(Utf8JsonWriter writer, ServerResult value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }
    }
}