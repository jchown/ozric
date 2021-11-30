using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    public class Colour
    {
        public float r { get; set;  }
        public float g { get; set;  }
        public float b { get; set;  }
        public float a { get; set;  }
        
        [JsonIgnore]
        public float luminance => (float) (0.3 * r + 0.59 * g + 0.11 * b);
    }
}