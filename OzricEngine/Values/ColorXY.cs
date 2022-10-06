using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine.Values
{
    /// <summary>
    /// A <see cref="ColorValue"/> defined by Hue and Saturation. 
    /// </summary>
    public sealed class ColorXY: ColorValue, IEquatable<ColorXY>
    {
        public static readonly ColorXY WHITE = new ColorXY(1f,1f,1f);

        public override ColorMode ColorMode => ColorMode.XY;

        public float x { get; }
        public float y { get; }

        public ColorXY()
        {
        }

        public ColorXY(float x, float y, float brightness): base(brightness)
        {
            this.x = x;
            this.y = y;
        }

        public override void GetRGB(out float r, out float g, out float b)
        {
            //  See https://gist.github.com/popcorn245/30afa0f98eea1c2fd34d
            
            float z = 1.0f - x - y;
            float Y = brightness; // The given brightness value
            float X = (Y / y) * x;
            float Z = (Y / y) * z;

            float _r = X * 1.4628067f - Y * 0.1840623f - Z * 0.2743606f;
            float _g = -X * 0.5217933f + Y * 1.4472381f + Z * 0.0677227f;
            float _b = X * 0.0349342f - Y * 0.0968930f + Z * 1.2884099f;

            // Apply reverse gamma correction

            r = _r <= 0.0031308f ? 12.92f * _r : (1.0f + 0.055f) * MathF.Pow(_r, (1.0f / 2.4f)) - 0.055f;
            g = _g <= 0.0031308f ? 12.92f * _g : (1.0f + 0.055f) * MathF.Pow(_g, (1.0f / 2.4f)) - 0.055f;
            b = _b <= 0.0031308f ? 12.92f * _b : (1.0f + 0.055f) * MathF.Pow(_b, (1.0f / 2.4f)) - 0.055f;
        }

        public static bool operator ==(ColorXY? lhs, ColorXY? rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(ColorXY? lhs, ColorXY? rhs) => !(lhs == rhs);
        
        public override bool Equals(object? o) => AreBothOff(o as ColorValue) || Equals(o as ColorXY);

        public bool Equals(ColorXY? other)
        {
            return (other != null) && (x == other.x && y == other.y && brightness == other.brightness);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, brightness);
        }
        
        public override string ToString()
        {
            return $"{x},{y} @ {((int) (brightness * 100))}%";
        }
        
        public override void WriteAsJSON(Utf8JsonWriter writer)
        {
            base.WriteAsJSON(writer);
            writer.WriteNumber("x", x);
            writer.WriteNumber("y", y);
        }

        public override ColorValue WithBrightness(float brightness)
        {
            return new ColorXY(x, y, brightness);
        }

        public new static ColorValue ReadFromJSON(ref Utf8JsonReader reader)
        {
            var brightness = ReadBrightnessFromJSON(ref reader);
            
            if (!reader.Read() || reader.GetString() != "x" || !reader.Read())
                throw new JsonException();

            float x = reader.GetSingle();
            
            if (!reader.Read() || reader.GetString() != "y"|| !reader.Read())
                throw new JsonException();

            float y = reader.GetSingle();

            return new ColorXY(x, y, brightness);
        }
    }
}