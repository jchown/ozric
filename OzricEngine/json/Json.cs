using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OzricEngine.ext;

namespace OzricEngine
{
    public class Json
    {
        public static string Serialize<T>(T t) where T: class
        {
            return Serialize(t, typeof(T));
        }
        
        public static string Serialize(object o, Type t)
        {
            return JsonSerializer.Serialize(o, t, Options);
        }
        
        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, Options)!;
        }
        
        /// <summary>
        /// Use JSON serialisation to take a deep copy of an object 
        /// </summary>
        /// <param name="original"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

        public static T Clone<T>(T original) where T: class
        {
            return Deserialize<T>(Serialize(original));
        }

        public delegate TObject CreateObject<out TObject>(ref Utf8JsonReader reader);

        public static TObject DeserializeViaEnum<TObject, TEnum>(ref Utf8JsonReader reader, IDictionary<TEnum, CreateObject<TObject>> creators) where TEnum : new()
        {
            if (!reader.Read() || reader.TokenType != JsonTokenType.String)
                throw new JsonException();
            
            string enumName = reader.GetString()!;
            if (!Enum.TryParse(typeof(TEnum), enumName, out var enumType))
                throw new JsonException($"Unknown {nameof(TEnum)} {enumName}");

            TEnum type = (TEnum) enumType!;
            var creator = creators.GetOrSet(type, () => throw new Exception($"No {nameof(TEnum)} creator for {enumName}"));
            TObject o = creator(ref reader);
            
            while (reader.TokenType != JsonTokenType.EndObject && reader.Read())
            {
            }

            return o;
        }
        
        public static string Prettify(string json)
        {
            return Prettify(JsonDocument.Parse(json));
        }

        public static string Prettify(JsonDocument jDoc)
        {
            return JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true });
        }

        static Json()
        {
            Options = new JsonSerializerOptions();
            Configure(Options);
        }

        public static readonly JsonSerializerOptions Options;

        public static void Configure(JsonSerializerOptions options)
        {
            options.PropertyNamingPolicy = new PropertyNamer();
            options.IncludeFields = true;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.Converters.Add(new JsonConverterAttributes());
            options.Converters.Add(new JsonConverterEvent()); 
            options.Converters.Add(new JsonConverterNode());
            options.Converters.Add(new JsonConverterClientCommand());
            options.Converters.Add(new JsonConverterServerMessage());
            options.Converters.Add(new JsonConverterValue());
//              new JsonConverterEntityID(),     - Only use explicitly
        }

        public class PropertyNamer : JsonNamingPolicy
        {
            public override string ConvertName(string name)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var ch in name)
                {
                    if (Char.IsUpper(ch) && sb.Length > 0)
                    {
                        sb.Append("-");
                        sb.Append(Char.ToLower(ch));
                    }
                    else
                    {
                        sb.Append(Char.ToLower(ch));
                    }
                }

                int len = sb.Length;
                if (len > 3)
                {
                    //  "ID" -> "-i-d" -> "-id"

                    if (sb[len - 3] == 'i' && sb[len - 2] == '-' && sb[len - 1] == 'd')
                    {
                        sb.Remove(len - 2, 1);
                    }
                }

                return sb.ToString();
            }
        }
    }
}