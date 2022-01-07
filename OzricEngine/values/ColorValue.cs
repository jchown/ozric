using System;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    public abstract class ColorValue: Value
    {
        public float brightness { get; }

        [JsonIgnore]
        public abstract float luminance { get; }

        protected ColorValue()
        {
        }

        protected ColorValue(float brightness)
        {
            this.brightness = brightness;
        }

        public virtual void GetRGB(out float r, out float g, out float b)
        {
            throw new NotImplementedException();
        }
    }
}