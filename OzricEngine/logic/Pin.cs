using System;

namespace OzricEngine.logic
{
    /// <summary>
    /// A named input or output, with a current value.
    /// </summary>
    public class Pin
    {
        public string name { get; }
        public object value { get; protected set;  }
        
        public Pin(string name, object value)
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
                
                case Colour c:
                    c.r = c.g = c.b = scalar.value;
                    return;
                
                case OnOff onOff:
                    onOff.value = scalar.value >= 0.5f;
                    return;
                
                default:
                    throw new Exception($"Don't know how to assign Scalar to {value.GetType()}");
            }
        }
        
        private void SetValue(Colour colour)
        {
            switch (value)
            {
                case Scalar s:
                    s.value = colour.luminance;
                    return;
                
                case Colour c:
                    c.r = colour.r;
                    c.g = colour.g;
                    c.b = colour.b;
                    c.a = colour.a;
                    return;
                
                case OnOff o:
                    o.value = colour.luminance >= 0.5f;
                    return;
                
                default:
                    throw new Exception($"Don't know how to assign Colour to {value.GetType()}");
            }
        }
        
        private void SetValue(OnOff onOff)
        {
            switch (value)
            {
                case Scalar s:
                    s.value = onOff.value ? 1 : 0;
                    return;
                
                case Colour c: 
                    c.r = c.g = c.b = onOff.value ? 1 : 0;
                    return;
                
                case OnOff o:
                    o.value = onOff.value;
                    return;
                
                default:
                    throw new Exception($"Don't know how to assign Colour to {value.GetType()}");
            }
        }

        public void SetValue(object value)
        {
            switch (value)
            {
                case Scalar scalar:
                    SetValue(scalar);
                    return;

                case Colour colour:
                    SetValue(colour);
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