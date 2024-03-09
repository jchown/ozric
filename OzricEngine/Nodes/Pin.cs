using System;
using System.Text.Json.Serialization;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

/// <summary>
/// A named input or output, with a current value.
/// </summary>
public class Pin : IGraphObject
{
    [JsonIgnore]
    public string id => name;
        
    public string name { get; set; }
    public ValueType type { get; set; }
    public Value? value { get; set; }

    public delegate void Changed(string nodeID, string pinName, Value value);

    public Pin(string name, ValueType type, Value? value = null)
    {
        this.name = name;
        this.type = type;
        this.value = value;
    }

    private void SetValue(Number number)
    {
        switch (type)
        {
            case ValueType.Number:
                value = number;
                return;

            case ValueType.Color:
                value = new ColorRGB(1, 1, 1, number.value);
                return;

            case ValueType.Binary:
                value = new Binary(number.value > 0);
                return;

            default:
                throw new Exception($"Don't know how to assign number to {type}");
        }
    }

    private void SetValue(ColorValue color)
    {
        switch (type)
        {
            case ValueType.Number:
                value = new Number(color.brightness);
                return;

            case ValueType.Color:
                value = color;
                return;

            case ValueType.Binary:
                value = new Binary(color.brightness > 0);
                return;

            default:
                throw new Exception($"Don't know how to assign Color to {type}");
        }
    }

    private void SetValue(Binary binary)
    {
        switch (type)
        {
            case ValueType.Number:
                value = new Number(binary.value ? 1 : 0);
                return;

            case ValueType.Color:
                value = new ColorRGB(1, 1, 1, binary.value ? 1 : 0);
                return;

            case ValueType.Binary:
                value = binary;
                return;

            default:
                throw new Exception($"Don't know how to assign Color to {type}");
        }
    }

    private void SetValue(Mode mode)
    {
        switch (type)
        {
            case ValueType.Mode:
                value = mode;
                return;

            default:
                throw new Exception($"Don't know how to assign Mode to {type}");
        }
    }

    public void SetValue(Value value)
    {
        switch (value)
        {
            case Number number:
                SetValue(number);
                return;

            case ColorValue color:
                SetValue(color);
                return;

            case Binary binary:
                SetValue(binary);
                return;

            case Mode mode:
                SetValue(mode);
                return;

            default:
                throw new Exception($"Don't know how to assign {value.GetType().Name}");
        }
    }
}