using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine.Values
{
    /// <summary>
    /// A <see cref="ColorValue"/> defined as Red, Green and Blue components. 
    /// </summary>
    public sealed class ColorRGB: ColorValue, IEquatable<ColorRGB>
    {
        public static readonly ColorRGB WHITE = new ColorRGB(1f,1f,1f,1f);
        public static readonly ColorRGB RED = new ColorRGB(1f,0f,0f,1f);
        public static readonly ColorRGB GREEN = new ColorRGB(0f,1f,0f,1f);
        public static readonly ColorRGB BLUE = new ColorRGB(0f,0f,1f,1f);
        public static readonly ColorRGB YELLOW = new ColorRGB(1f,1f,1f,1f);

        public override ColorMode ColorMode => ColorMode.RGB;

        public class Serialized
        {
            public string rgb { get; set; }
            public float brightness { get; set; }
        }

        /// <summary>
        /// 0-1
        /// </summary>
        public float r { get; }
        
        /// <summary>
        /// 0-1
        /// </summary>
        public float g { get; }
        
        /// <summary>
        /// 0-1
        /// </summary>
        public float b { get; }

        public ColorRGB(float r, float g, float b, float brightness): base(brightness)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public ColorRGB(int rgb, float brightness = 1f): base(brightness)
        {
            b = (rgb & 0xff) / 255f;
            g = ((rgb >> 8) & 0xff) / 255f;
            r = ((rgb >> 16) & 0xff) / 255f;
        }
        
        public override void GetRGB(out float r, out float g, out float b)
        {
            r = this.r;
            g = this.g;
            b = this.b;
        }

        public static bool operator ==(ColorRGB? lhs, ColorRGB? rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(ColorRGB? lhs, ColorRGB? rhs) => !(lhs == rhs);
        
        public override bool Equals(object? o) => AreBothOff(o as ColorValue) || Equals(o as ColorRGB);

        public bool Equals(ColorRGB? other)
        {
            return (other != null) && (r == other.r && g == other.g && b == other.b && brightness == other.brightness);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(r, g, b, brightness);
        }
        
        public override string ToString()
        {
            if (brightness == 0)
                return "off";
            
            return $"{ToRGB24().ToString("X6")} @ {(int) (brightness * 100)}%";
        }

        public int ToRGB24()
        {
            int r8 = (int)(r * 255f + 0.5f);
            int g8 = (int)(g * 255f + 0.5f);
            int b8 = (int)(b * 255f + 0.5f);
            return (r8 << 16) | (g8 << 8) | b8;
        }

        private string ToHex(float f)
        {
            return ((int) (f * 255 + 0.5f)).ToString("X2");
        }

        public override void WriteAsJSON(Utf8JsonWriter writer)
        {
            base.WriteAsJSON(writer);
            writer.WriteString("rgb", $"{ToHex(r)}{ToHex(g)}{ToHex(b)}");
        }

        public override ColorValue WithBrightness(float brightness)
        {
            return new ColorRGB(r, g, b, brightness);
        }

        public new static ColorValue ReadFromJSON(ref Utf8JsonReader reader)
        {
            var brightness = ReadBrightnessFromJSON(ref reader);
            
            if (!reader.Read() || reader.GetString() != "rgb"|| !reader.Read())
                throw new JsonException();

            return FromHex(reader.GetString(), brightness);
        }

        public static ColorRGB FromHex(string hexString, float brightness)
        {
            var r = FromHex(hexString, 0);
            var g = FromHex(hexString, 2);
            var b = FromHex(hexString, 4);
            return new ColorRGB(r, g, b, brightness);
        }

        private static float FromHex(string hexString, int offset)
        {
            return (FromHex(hexString[offset]) * 16 + FromHex(hexString[offset + 1])) / 255f;
        }

        private static int FromHex(char hexChar)
        {
            switch (hexChar)
            {
                case var ch when ch >= '0' && ch <= '9':
                    return ch - '0';
                
                case var ch when ch >= 'a' && ch <= 'f':
                    return ch - 'a' + 10;
                
                case var ch when ch >= 'A' && ch <= 'F':
                    return ch - 'A' + 10;
                
                default:
                    throw new Exception($"{hexChar} is not a hex digit");
            }
        }
    }
}