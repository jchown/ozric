using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine
{
    /// <summary>
    /// Convert a dictionary of object values into their natural JSON equivalents and back again.
    /// See https://josef.codes/custom-dictionary-string-object-jsonconverter-for-system-text-json/
    /// </summary>
    public class JsonConverterAttributes : JsonConverter<Attributes>
    {
        public override Attributes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"JsonTokenType was of type {reader.TokenType}, only objects are supported");
            }

            var dictionary = new Attributes();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("JsonTokenType was not PropertyName");
                }

                var propertyName = reader.GetString();

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new JsonException("Failed to get property name");
                }

                reader.Read();

                dictionary.Add(propertyName, ExtractValue(ref reader, options));
            }

            return dictionary;
        }

        public override void Write(Utf8JsonWriter writer, Attributes value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (Dictionary<string, object>) value, options);
        }

        private object ExtractValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    if (reader.TryGetDateTime(out var date))
                        return date;

                    return reader.GetString();

                case JsonTokenType.False:
                    return false;

                case JsonTokenType.True:
                    return true;

                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.Number:
                    if (reader.TryGetInt32(out var i))
                        return i;

                    if (reader.TryGetSingle(out var f))
                        return f;

                    return reader.GetDecimal();

                case JsonTokenType.StartObject:
                    return Read(ref reader, null, options);

                case JsonTokenType.StartArray:
                    var list = new List<object>();
                    Type type = null;
                    bool setType = false;
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        var item = ExtractValue(ref reader, options);

                        if (!setType)
                        {
                            if (item != null)
                            {
                                setType = true;
                                type = item.GetType();
                            }
                        }
                        else if (type != item.GetType())
                            type = null;
                        
                        list.Add(item);
                    }

                    if (type != null)
                    {
                        var listType = typeof(List<>).MakeGenericType(type);
                        IList typed = (IList)Activator.CreateInstance(listType);
                        
                        foreach (var item in list)
                            typed.Add(item);

                        return typed;
                    }
    
                    return list;
                
                default:
                    throw new JsonException($"'{reader.TokenType}' is not supported");
            }
        }
    }
}