using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine.Values
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

        public ColorTemp(float temp, float brightness): base(brightness)
        {
            this.temp = temp;
        }

        public float temp { get; }

        public static bool operator ==(ColorTemp? lhs, ColorTemp? rhs)
        {
            if (lhs is null)
                return rhs is null;

            return lhs.Equals(rhs);
        }
        
        public static bool operator !=(ColorTemp? lhs, ColorTemp? rhs) => !(lhs == rhs);
        
        public override bool Equals(object? o) => AreBothOff(o as ColorValue) || Equals(o as ColorTemp);

        public bool Equals(ColorTemp? other)
        {
            return (other != null) && (temp == other.temp && brightness == other.brightness);
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

        public override void GetRGB(out float r, out float g, out float b)
        {
            //  See https://stackoverflow.com/questions/60777515/how-can-i-convert-mireds-to-rgb-or-display-mireds-in-css/
            //  then https://gist.github.com/paulkaplan/5184275
            
            double kelvin = 10e6f / temp;
            var t = kelvin / 100;
            double red, green, blue;

            if( t <= 66 ){ 

                red = 255; 
                green = t;
                green = 99.4708025861 * Math.Log(green) - 161.1195681661;
        
                if( t <= 19){

                    blue = 0;

                } else {

                    blue = t-10;
                    blue = 138.5177312231 * Math.Log(blue) - 305.0447927307;

                }

            } else {

                red = t - 60;
                red = 329.698727446 * Math.Pow(red, -0.1332047592);
        
                green = t - 60;
                green = 288.1221695283 * Math.Pow(green, -0.0755148492 );

                blue = 255;
            }

            r = (float)Math.Clamp(red / 255, 0, 1);
            g = (float)Math.Clamp(green / 255, 0, 1);
            b = (float)Math.Clamp(blue / 255, 0, 1);
        }

        public override void WriteAsJSON(Utf8JsonWriter writer)
        {
            base.WriteAsJSON(writer);    
            writer.WriteNumber("temp", temp);
        }

        public override ColorValue WithBrightness(float brightness)
        {
            return new ColorTemp(temp, brightness);
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