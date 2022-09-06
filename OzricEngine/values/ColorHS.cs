using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    /// <summary>
    /// A <see cref="ColorValue"/> defined by Hue and Saturation. 
    /// </summary>
    public sealed class ColorHS: ColorValue, IEquatable<ColorHS>
    {
        public static readonly ColorHS WHITE = new ColorHS(1f,1f,1f);

        public override ColorMode ColorMode => ColorMode.HS;

        public float h { get; }
        public float s { get; }

        public ColorHS()
        {
        }

        public ColorHS(float h, float s, float brightness): base(brightness)
        {
            this.h = h;
            this.s = s;
        }

        [JsonIgnore]
        public override float luminance => (float)(0.3 * h + 0.59 * s + 0.11);

        public override void GetRGB(out float r, out float g, out float b)
        {
            //  See https://stackoverflow.com/questions/3018313/algorithm-to-convert-rgb-to-hsv-and-hsv-to-rgb-in-range-0-255-for-both
            
            float H = h * 6;
            float fract = H - MathF.Floor(H);

            float P = (1 - s);
            float Q = (1 - s * fract);
            float T = (1 - s * (1 - fract));

            if (H < 1)
            {
                r = 1;
                g = T;
                b = P;
            }
            else if (H < 2)
            {
                r = Q;
                g = 1;
                b = P;
            }
            else if (H < 3)
            {
                r = P;
                g = 1;
                b = T;
            }
            else if (H < 4)
            {
                r = P;
                g = Q;
                b = 1;
            }
            else if (H < 5)
            {
                r = T;
                g = P;
                b = 1;
            }
            else
            {
                r = 1;
                g = P;
                b = Q;
            }
        }

        public static bool operator ==(ColorHS? lhs, ColorHS? rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(ColorHS? lhs, ColorHS? rhs) => !(lhs == rhs);
        
        public override bool Equals(object? o) => Equals(o as ColorHS);

        public bool Equals(ColorHS? other)
        {
            return (other != null) && (h == other.h && s == other.s && brightness == other.brightness);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(h, s, brightness);
        }
        
        public override string ToString()
        {
            return $"{h},{s} @ {((int) (brightness * 100))}%";
        }
        
        public override void WriteAsJSON(Utf8JsonWriter writer)
        {
            base.WriteAsJSON(writer);
            writer.WriteNumber("h", h);
            writer.WriteNumber("s", s);
        }

        public new static ColorValue ReadFromJSON(ref Utf8JsonReader reader)
        {
            var brightness = ReadBrightnessFromJSON(ref reader);
            
            if (!reader.Read() || reader.GetString() != "h" || !reader.Read())
                throw new JsonException();

            float h = reader.GetSingle();
            
            if (!reader.Read() || reader.GetString() != "s"|| !reader.Read())
                throw new JsonException();

            float s = reader.GetSingle();

            return new ColorHS(h, s, brightness);
        }
    }
}