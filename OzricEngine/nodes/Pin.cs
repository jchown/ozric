using System;

namespace OzricEngine.logic
{
    /// <summary>
    /// A named input or output, with a current value.
    /// </summary>
    public class Pin
    {
        public string name { get; }
        public ValueType type { get; }
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
                
                case ValueType.OnOff:
                    value = new OnOff(scalar.value > 0);
                    return;
                
                default:
                    throw new Exception($"Don't know how to assign Scalar to {value.GetType()}");
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

                case ValueType.OnOff:
                    value = new OnOff(color.brightness > 0);
                    return;
                
                default:
                    throw new Exception($"Don't know how to assign Color to {this.value.GetType()}");
            }
        }
        
        private void SetValue(OnOff onOff)
        {
            switch (type)
            {
                case ValueType.Scalar:
                    value = new Scalar(onOff.value ? 1 : 0);
                    return;
                
                case ValueType.Color: 
                    value = new ColorRGB(1,1,1, onOff.value ? 1 : 0);
                    return;
                
                case ValueType.OnOff:
                    value = onOff;
                    return;
                
                default:
                    throw new Exception($"Don't know how to assign Color to {value.GetType()}");
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

                case OnOff onOff:
                    SetValue(onOff);
                    return;

                default:
                    throw new Exception($"Don't know how to assign {value.GetType().Name}");
            }
        }
    }
}