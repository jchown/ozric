using System;

namespace OzricEngine.logic
{
    /// <summary>
    /// A named input or output, with a current value.
    /// </summary>
    public class Pin
    {
        public string name { get; }
        public Value value { get; }
        
        public Pin(string name, Value value)
        {
            this.name = name;
            this.value = value;
        }
        
        private void SetValue(Scalar scalar)
        {
            switch (value)
            {
                case Scalar s:
                    s.value = scalar.value;
                    return;
                
                case ColorRGB c:
                    c.r = c.g = c.b = scalar.value;
                    return;
                
                case OnOff onOff:
                    onOff.value = scalar.value >= 0.5f;
                    return;
                
                default:
                    throw new Exception($"Don't know how to assign Scalar to {value.GetType()}");
            }
        }
        
        private void SetValue(ColorValue value)
        {
            switch (this.value)
            {
                case Scalar s:
                    s.value = value.luminance;
                    return;

                case ColorRGB c:

                    // TODO: Sod this off and make inputs have a class "type", not a value, and just verify & copy. No interpolations.

                    if (value is ColorRGB rgb)
                    {
                        c.r = value.r;
                        c.g = value.g;
                        c.b = value.b;
                    }
                    c.brightness = value.brightness;
                    return;

                case ColorRGB c:
                    c.r = value.r;
                    c.g = value.g;
                    c.b = value.b;
                    return;

                case OnOff o:
                    o.value = value.luminance >= 0.5f;
                    return;
                
                default:
                    throw new Exception($"Don't know how to assign Color to {this.value.GetType()}");
            }
        }
        
        private void SetValue(OnOff onOff)
        {
            switch (value)
            {
                case Scalar s:
                    s.value = onOff.value ? 1 : 0;
                    return;
                
                case ColorValue c: 
                    c.luminance = onOff.value ? 1 : 0;
                    return;
                
                case OnOff o:
                    o.value = onOff.value;
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