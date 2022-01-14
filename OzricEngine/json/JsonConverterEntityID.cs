using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine
{
    /// <summary>
    /// Convert no item, a string or a list of strings to a list of strings
    /// </summary>
    public class JsonConverterEntityID : JsonConverter<List<string>>
    {
        public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.StartArray:
                {
                    var list = new List<string>();
                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.EndArray:
                                return list;
                           
                            case JsonTokenType.String:
                                list.Add(reader.GetString());
                                break;
                            
                            default:
                                throw new Exception($"Unexpected entity_id array content type, {reader.TokenType}");
                        }
                    }

                    return list;
                }

                case JsonTokenType.String:
                    return new List<string> { reader.GetString() };
                
                default:
                    throw new Exception($"Unexpected type for entity ID: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}