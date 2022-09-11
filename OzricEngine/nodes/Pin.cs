using System;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    /// <summary>
    /// A named input or output, with a current value.
    /// </summary>
    public class Pin : IGraphObject
    {
        [JsonIgnore]
        public string id => name;
        
        public string name { get; set; }
        public ValueType type { get; set; }
        public Value value { get; set; }

        public Pin(string name, ValueType type, Value value = null)
        {
            this.name = name;
            this.type = type;
            this.value = value;
        }

        private void SetValue(Scalar scalar)
        {
            switch (type)
            {
                case ValueType.Scalar:
                    value = scalar;
                    return;

                case ValueType.Color:
                    value = new ColorRGB(1, 1, 1, scalar.value);
                    return;

                case ValueType.Boolean:
                    value = new Boolean(scalar.value > 0);
                    return;

                default:
                    throw new Exception($"Don't know how to assign Scalar to {type}");
            }
        }

        private void SetValue(ColorValue color)
        {
            switch (type)
            {
                case ValueType.Scalar:
                    value = new Scalar(color.brightness);
                    return;

                case ValueType.Color:
                    value = color;
                    return;

                case ValueType.Boolean:
                    value = new Boolean(color.brightness > 0);
                    return;

                default:
                    throw new Exception($"Don't know how to assign Color to {type}");
            }
        }

        private void SetValue(Boolean boolean)
        {
            switch (type)
            {
                case ValueType.Scalar:
                    value = new Scalar(boolean.value ? 1 : 0);
                    return;

                case ValueType.Color:
                    value = new ColorRGB(1, 1, 1, boolean.value ? 1 : 0);
                    return;

                case ValueType.Boolean:
                    value = boolean;
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
                case Scalar scalar:
                    SetValue(scalar);
                    return;

                case ColorValue Color:
                    SetValue(Color);
                    return;

                case Boolean onOff:
                    SetValue(onOff);
                    return;

                case Mode mode:
                    SetValue(mode);
                    return;

                default:
                    throw new Exception($"Don't know how to assign {value.GetType().Name}");
            }
        }
    }
}