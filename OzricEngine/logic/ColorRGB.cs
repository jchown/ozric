using System;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    public sealed class ColorRGB: ColorValue, IEquatable<ColorRGB>
    {
        public static readonly ColorRGB WHITE = new ColorRGB(1f,1f,1f,1f);

        public ColorRGB()
        {
            r = g = b = 1;
            brightness = 1;
        }

        public ColorRGB(int rgb, float brightness = 1f)
        {
            b = (rgb & 0xff) / 255f;
            g = ((rgb >> 8) & 0xff) / 255f;
            r = ((rgb >> 16) & 0xff) / 255f;
            this.brightness = brightness;
        }

        public ColorRGB(float r, float g, float b, float brightness)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.brightness = brightness;
        }

        public float r { get; set;  }
        public float g { get; set;  }
        public float b { get; set;  }
        
        [JsonIgnore]
        public override float luminance
        {
            get => (float)(0.3 * r + 0.59 * g + 0.11 * b) * brightness;
            set
            {
                r = g = b = 1;
                brightness = value;
            }
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