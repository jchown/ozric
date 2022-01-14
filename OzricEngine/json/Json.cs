using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Humanizer;
using OzricEngine.ext;
using OzricEngine.logic;
using ValueType = System.ValueType;

namespace OzricEngine
{
    public class Json
    {
        public static string Serialize<T>(T t)
        {
            return Serialize(t, typeof(T));
        }
        
        public static string Serialize(object o, Type t)
        {
            return JsonSerializer.Serialize(o, t, options);
        }
        
        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }

        public delegate TObject CreateObject<out TObject>(ref Utf8JsonReader reader);

        public static TObject DeserializeViaEnum<TObject, TEnum>(ref Utf8JsonReader reader, IDictionary<TEnum, CreateObject<TObject>> creators) where TEnum : new()
        {
            if (!reader.Read() || reader.TokenType != JsonTokenType.String)
                throw new Exception();
            
            string enumName = reader.GetString();
            if (!Enum.TryParse(typeof(TEnum), enumName, out var enumType))
                throw new Exception($"Unknown {nameof(TEnum)} {enumName}");

            TEnum type = (TEnum) enumType;
            var creator = creators.GetOrSet(type, () => throw new Exception($"No {nameof(TEnum)} creator for {enumName}"));
            TObject o = creator(ref reader);
            
            while (reader.TokenType != JsonTokenType.EndObject && reader.Read())
            {
            }

            return o;
        }

        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            Converters =
            {
                new JsonConverterAttributes(),
//              new JsonConverterEntityID(),     - Only use explicitly
                new JsonConverterEvent(),
                new JsonConverterNode(),
                new JsonConverterServerMessage(),
                new JsonConverterValue(),
            }
        };
    }
}