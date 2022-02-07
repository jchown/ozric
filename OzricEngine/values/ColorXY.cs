using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    /// <summary>
    /// A <see cref="ColorValue"/> defined by Hue and Saturation. 
    /// </summary>
    public sealed class ColorXY: ColorValue, IEquatable<ColorXY>
    {
        public static readonly ColorXY WHITE = new ColorXY(1f,1f,1f);

        public override ColorType ColorType => ColorType.XY;

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

        [JsonIgnore]
        public override float luminance => (float)(0.3 * x + 0.59 * y + 0.11);

        public override void GetRGB(out float r, out float g, out float b)
        {
            throw new Exception();
        }

        public static bool operator ==(ColorXY lhs, ColorXY rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(ColorXY lhs, ColorXY rhs) => !(lhs == rhs);
        
        public override bool Equals(object o) => Equals(o as ColorXY);

        public bool Equals(ColorXY other)
        {
            return (other != null) && x == other.x && y == other.y && brightness == other.brightness;
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