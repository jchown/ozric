using System;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    public sealed class ColorRGB: ColorValue, IEquatable<ColorRGB>
    {
        public static readonly ColorRGB WHITE = new ColorRGB(1f,1f,1f,1f);
        public static readonly ColorRGB RED = new ColorRGB(1f,0f,0f,1f);
        public static readonly ColorRGB GREEN = new ColorRGB(0f,1f,0f,1f);
        public static readonly ColorRGB BLUE = new ColorRGB(0f,0f,1f,1f);
        public static readonly ColorRGB YELLOW = new ColorRGB(1f,1f,1f,1f);

        public ColorRGB()
        {
        }

        public ColorRGB(int rgb, float brightness = 1f): base(brightness)
        {
            b = (rgb & 0xff) / 255f;
            g = ((rgb >> 8) & 0xff) / 255f;
            r = ((rgb >> 16) & 0xff) / 255f;
        }

        public ColorRGB(float r, float g, float b, float brightness): base(brightness)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public float r { get; }
        public float g { get; }
        public float b { get; }
        
        [JsonIgnore]
        public override float luminance => (float)(0.3 * r + 0.59 * g + 0.11 * b) * brightness;

        public override void GetRGB(out float r, out float g, out float b)
        {
            r = this.r;
            g = this.g;
            b = this.b;
        }
        
        public override bool Equals(object o) => Equals(o as ColorRGB);

        public bool Equals(ColorRGB other)
        {
            return (other != null) && (r == other.r && g == other.g && b == other.b && brightness == other.brightness);
        }

        public override string ToString()
        {
            return $"{ToHex(r)}{ToHex(g)}{ToHex(b)} @ {((int) (brightness * 100))}%";
        }

        private string ToHex(float x)
        {
            return ((int) (x * 255)).ToString("X2");
        }
    }
}