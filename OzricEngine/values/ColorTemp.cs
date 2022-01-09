using System;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    public sealed class ColorTemp: ColorValue, IEquatable<ColorTemp>
    {
        public ColorTemp()
        {
        }

        public ColorTemp(int t, float brightness): base(brightness)
        {
            this.t = t;
        }

        public int t { get; }
        
        [JsonIgnore]
        public override float luminance => brightness;

        public static bool operator ==(ColorTemp lhs, ColorTemp rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(ColorTemp lhs, ColorTemp rhs) => !(lhs == rhs);
        
        public override bool Equals(object o) => Equals(o as ColorTemp);

        public bool Equals(ColorTemp other)
        {
            return other != null && t == other.t;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(t, brightness);
        }
        
        public override string ToString()
        {
            if (brightness == 0)
                return "off";
            
            return $"{t:F1} @ {(int) (brightness * 100)}%";
        }
    }
}