using System;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    public sealed class Colour: IEquatable<Colour>
    {
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