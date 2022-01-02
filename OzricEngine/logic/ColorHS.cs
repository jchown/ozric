using System;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    public sealed class ColorHS: ColorValue, IEquatable<ColorHS>
    {
        public static readonly ColorHS WHITE = new ColorHS(1f,1f);

        public float h { get; }
        public float s { get; }

        public ColorHS()
        {
            h = s = 0;
        }

        public ColorHS(float h, float s)
        {
            this.h = h;
            this.s = s;
        }

        [JsonIgnore]
        public override float luminance
        {
            get => (float)(0.3 * h + 0.59 * s + 0.11);
            set => brightness = value;
        }

        public override bool Equals(object o) => Equals(o as ColorHS);

        public bool Equals(ColorHS other)
        {
            return (other != null) && (h == other.h && s == other.s);
        }

        public override string ToString()
        {
            return $"{h},{s}";
        }
    }
}