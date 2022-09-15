using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OzricEngine.Values;
using Boolean = OzricEngine.Values.Boolean;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine
{
    /// <summary>
    /// Value deserializer
    /// </summary>
    public class JsonConverterValue: JsonConverter<Value>
    {
        static readonly Dictionary<ValueType, Json.CreateObject<Value>> creators = new Dictionary<ValueType, Json.CreateObject<Value>>
        {
            { ValueType.Boolean, Boolean.ReadFromJSON },
            { ValueType.Mode, Mode.ReadFromJSON },
            { ValueType.Scalar, Scalar.ReadFromJSON },
            { ValueType.Color, ColorValue.ReadFromJSON }
        };
        
        public override Value Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject || !reader.Read())
                throw new Exception();

            if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "value-type")
                throw new Exception();

            return Json.DeserializeViaEnum(ref reader, creators);
        }

        public override void Write(Utf8JsonWriter writer, Value value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("value-type", value.ValueType.ToString());
            value.WriteAsJSON(writer);
            writer.WriteEndObject();
        }
    }
}