using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    /// <summary>
    /// A <see cref="ColorValue"/> as a (white) temperature. 
    /// </summary>
    public sealed class ColorTemp: ColorValue, IEquatable<ColorTemp>
    {
        public override ColorMode ColorMode => ColorMode.Temp;

        public ColorTemp()
        {
        }

        public ColorTemp(int temp, float brightness): base(brightness)
        {
            this.temp = temp;
        }

        public int temp { get; }
        
        [JsonIgnore]
        public override float luminance => brightness;

        public static bool operator ==(ColorTemp? lhs, ColorTemp? rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(ColorTemp? lhs, ColorTemp? rhs) => !(lhs == rhs);
        
        public override bool Equals(object o) => Equals(o as ColorTemp);

        public bool Equals(ColorTemp? other)
        {
            return (other != null) && temp == other.temp;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(temp, brightness);
        }
        
        public override string ToString()
        {
            if (brightness == 0)
                return "off";
            
            return $"{temp:F1} @ {(int) (brightness * 100)}%";
        }

        public override void WriteAsJSON(Utf8JsonWriter writer)
        {
            base.WriteAsJSON(writer);    
            writer.WriteNumber("temp", temp);
        }
        
        public new static ColorValue ReadFromJSON(ref Utf8JsonReader reader)
        {
            var brightness = ReadBrightnessFromJSON(ref reader);
            
            if (!reader.Read() || reader.GetString() != "temp" || !reader.Read())
                throw new JsonException();

            return new ColorTemp(reader.GetInt32(), brightness);
        }
    }
}