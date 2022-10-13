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
    public override bool CanConvert(Type typeToConvert) =>
        typeof(Value).IsAssignableFrom(typeToConvert);
    
    private const string valueKey = "value-type";
    private const string colorKey = "color-type";
    
    static readonly Dictionary<ValueType, Type> nonColorCreators = new()
    {
        { ValueType.Boolean, typeof(Boolean) },
        { ValueType.Mode, typeof(Mode) },
        { ValueType.Scalar, typeof(Scalar) }
    };

    static readonly Dictionary<ColorMode, Type> colorCreators = new()
    {
        { ColorMode.XY, typeof(ColorXY) },
        { ColorMode.HS, typeof(ColorHS) },
        { ColorMode.Temp, typeof(ColorTemp) },
        { ColorMode.RGB, typeof(ColorRGB.Serialized) }  //  Special case, see below
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
            if (!Enum.TryParse(typeof(ValueType), valueTypeName, out var vt))
                throw new JsonException($"Unknown {nameof(ValueType)} {valueTypeName}");

            var valueType = (ValueType) vt!;
            switch (valueType)
            {
                case ValueType.Color:
                {
                    //  Need to do an extra look-up based on the color mode to determine final type
                    
                    if (!jsonDocument.RootElement.TryGetProperty(colorKey, out var modeProperty))
                        throw new JsonException($"Missing {colorKey} in {jsonDocument}");

                    var colorModeName = modeProperty.GetString()!;
                    if (!Enum.TryParse(typeof(ColorMode), colorModeName, out var cm))
                        throw new JsonException($"Unknown {nameof(ColorMode)} {colorModeName}");

                    var colorMode = (ColorMode) cm!;
                    var type = colorCreators[colorMode];
                    var jsonObject = jsonDocument.RootElement.GetRawText();

                    switch (colorMode)
                    {
                        case ColorMode.RGB:
                        {
                            //  I was a smart arse and encoded the channels as hex, sorry...

                            var intermediate = (ColorRGB.Serialized) JsonSerializer.Deserialize(jsonObject, type, options)!;
                            return ColorRGB.FromHex(intermediate.rgb, intermediate.brightness);
                        }
                        default:
                        {
                            return (Value) JsonSerializer.Deserialize(jsonObject, type, options)!;
                        }
                    }
                }
                
                default:
                {
                    var type = nonColorCreators[valueType];
                    var jsonObject = jsonDocument.RootElement.GetRawText();
                    return (Value) JsonSerializer.Deserialize(jsonObject, type, options)!;
                }
            }
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