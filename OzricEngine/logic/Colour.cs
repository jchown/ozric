using System;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    public sealed class Colour: IEquatable<Colour>
    {
        public static readonly Colour WHITE = new Colour(1f,1f,1f);

        public Colour()
        {
            r = g = b = a = 1f;
        }

        public Colour(int rgb)
        {
            a = 1f;
            b = (rgb & 0xff) / 255f;
            g = ((rgb >> 8) & 0xff) / 255f;
            r = ((rgb >> 16) & 0xff) / 255f;
        }

        public Colour(float r, float g, float b, float a = 1f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public float r { get; set;  }
        public float g { get; set;  }
        public float b { get; set;  }
        public float a { get; set;  }
        
        [JsonIgnore]
        public float luminance => (float) (0.3 * r + 0.59 * g + 0.11 * b);
        
        public override bool Equals(object o) => this.Equals(o as Colour);

        public bool Equals(Colour other)
        {
            return (other != null) && (r == other.r && g == other.g && b == other.b && a == other.a);
        }
    }
}