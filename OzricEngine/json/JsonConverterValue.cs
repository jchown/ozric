using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OzricEngine.Values;
using Boolean = OzricEngine.Values.Boolean;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine;

/// <summary>
/// Value deserializer. Because there isn't a simple field <-> type key (because Color has several derivatives), we need a custom deserializer
/// </summary>
public class JsonConverterValue: JsonConverter<Value>
{
    private const string valueKey = "value-type";
    private const string colorKey = "color-type";

    public override bool CanConvert(Type typeToConvert) => typeof(Value).IsAssignableFrom(typeToConvert);

    public delegate Value ValueCreator(JsonDocument document);

    static readonly Dictionary<String, ValueCreator> creators = new()
    {
        { nameof(ValueType.Boolean), Boolean.ReadFromJSON },
        { nameof(ValueType.Mode), Mode.ReadFromJSON },
        { nameof(ValueType.Scalar), Scalar.ReadFromJSON },
        { nameof(ColorMode.XY), ColorXY.ReadFromJSON },
        { nameof(ColorMode.HS), ColorHS.ReadFromJSON },
        { nameof(ColorMode.Temp), ColorTemp.ReadFromJSON },
        { nameof(ColorMode.RGB), ColorRGB.ReadFromJSON }
    };

    public override Value Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        using (var jsonDocument = JsonDocument.ParseValue(ref reader))
        {
            if (!jsonDocument.RootElement.TryGetProperty(valueKey, out var typeProperty))
                throw new JsonException($"Missing {valueKey} in {jsonDocument}");

            var valueTypeName = typeProperty.GetString()!;
            if (!Enum.TryParse(typeof(ValueType), valueTypeName, out _))
                throw new JsonException($"Unknown {nameof(ValueType)} {valueTypeName}");

            if (valueTypeName == "Color")
            {
                //  Need to do an extra look-up based on the color mode to determine final type

                if (!jsonDocument.RootElement.TryGetProperty(colorKey, out var modeProperty))
                    throw new JsonException($"Missing {colorKey} in {jsonDocument}");

                valueTypeName = modeProperty.GetString()!;
                if (!Enum.TryParse(typeof(ColorMode), valueTypeName, out _))
                    throw new JsonException($"Unknown {nameof(ColorMode)} {valueTypeName}");
            }

            return creators[valueTypeName].Invoke(jsonDocument);
        }
    }

    public override void Write(Utf8JsonWriter writer, Value value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("value-type", value.ValueType.ToString());
        value.WriteAsJSON(writer);
        writer.WriteEndObject();
    }
}