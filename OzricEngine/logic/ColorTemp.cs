using System;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    public sealed class ColorTemp: ColorValue, IEquatable<ColorTemp>
    {
        public ColorTemp()
        {
        }

        public ColorTemp(int t)
        {
            this.t = t;
        }

        public int t { get; }
        
        [JsonIgnore]
        public override float luminance
        {
            get => brightness;
            set => brightness = value;
        }

        public override bool Equals(object o) => Equals(o as ColorTemp);

        public bool Equals(ColorTemp other)
        {
            return (other != null) && (t == other.t);
        }
    }
}