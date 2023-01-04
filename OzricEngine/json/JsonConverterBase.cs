using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine
{
    /// <summary>
    /// Generic JSON deserializer (only) for an abstract base type, using a specified field to indicate derived type. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonConverterBase<T>: JsonConverter<T>
    {
//        public override bool CanConvert(Type typeToConvert) => typeof(T).IsAssignableFrom(typeToConvert);

        private readonly IDictionary<string, Type> ResultTypes = new Dictionary<string, Type>();
        private readonly string key;

        public JsonConverterBase(string key)
        {
            this.key = key;
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => typeof(T).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract))
            {
                var attr = type.GetCustomAttribute<TypeKeyAttribute>();
                if (attr == null)
                    throw new Exception($"No {typeof(TypeKeyAttribute)} attribute on {type}");

                if (attr.value == null)
                    throw new Exception($"Null value for {typeof(TypeKeyAttribute)} attribute on {type}");

                if (ResultTypes.ContainsKey(attr.value))
                    throw new Exception($"Duplicate value {attr.value} for {typeof(TypeKeyAttribute)} attribute on {type}, previously {ResultTypes[attr.value]}");

                ResultTypes[attr.value] = type;
            }
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            using (var jsonDocument = JsonDocument.ParseValue(ref reader))
            {
                if (!jsonDocument.RootElement.TryGetProperty(key, out var typeProperty))
                    throw new JsonException($"Missing {key} in {jsonDocument}");

                var value = typeProperty.GetString()!;
                if (!ResultTypes.ContainsKey(value))
                    return OnUnrecognisedType(jsonDocument, value);
                    
                var type = ResultTypes[value];
                var jsonObject = jsonDocument.RootElement.GetRawText();
                return (T) JsonSerializer.Deserialize(jsonObject, type, options)!;
            }
        }

        protected virtual T OnUnrecognisedType(JsonDocument doc, string name)
        {
            throw new JsonException($"Unknown class for \"{name}\" of {typeof(T)}");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object?) value, options);
        }
    }
}