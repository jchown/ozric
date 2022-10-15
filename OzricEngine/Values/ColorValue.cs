using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine.Values
{
    /// <summary>
    /// Base class for all colors. All color representations (e.g. <see cref="ColorRGB"/>, <see cref="ColorHS"/> etc.)
    /// have a single <see cref="ValueType" /> of "Color" and use <see cref="ColorMode"/> to distinguish them.
    /// Formally speaking we have several ways of defining the chrominance.
    /// We separate this from the luminance, which we refer to as <see cref="brightness"/>.
    /// A brightness of 0 for lights is considered as "off". 
    /// </summary>
    /// <seealso cref="https://en.wikipedia.org/wiki/Chrominance"/>

    public abstract class ColorValue: Value
    {
        public override ValueType ValueType => ValueType.Color;

        public abstract ColorMode ColorMode { get; }

        /// <summary>
        /// 0-1 
        /// </summary>
        public float brightness { get; set; }

        protected ColorValue(float brightness)
        {
            this.brightness = brightness;
        }

        private static readonly Dictionary<ColorMode, Json.CreateObject<ColorValue>> creators = new()
        {
            { ColorMode.HS, ColorHS.ReadFromJSON },
            { ColorMode.RGB, ColorRGB.ReadFromJSON },
            { ColorMode.Temp, ColorTemp.ReadFromJSON },
            { ColorMode.XY, ColorXY.ReadFromJSON },
        };
            
        public static ColorValue ReadFromJSON(ref Utf8JsonReader reader)
        {
            if (!reader.Read() || reader.GetString() != "color-type")
                throw new JsonException();

            return Json.DeserializeViaEnum(ref reader, creators);
        }

        protected static float ReadBrightnessFromJSON(ref Utf8JsonReader reader)
        {
            if (!reader.Read() || reader.GetString() != "brightness" || !reader.Read())
                throw new JsonException();

            return reader.GetSingle();
        }

        /// <summary>
        /// Return this color in the nearest equivalent RGB value, in the range 0-1
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public abstract void GetRGB(out float r, out float g, out float b);

        public override void WriteAsJSON(Utf8JsonWriter writer)
        {
            writer.WriteString("color-type", ColorMode.ToString());
            writer.WriteNumber("brightness", brightness);
        }

        public string ToHexString()
        {
            GetRGB(out var r, out var g, out var b);
            int ri = (int)(r * 255f);
            int gi = (int)(g * 255f);
            int bi = (int)(b * 255f);
            return $"{ri:X2}{gi:X2}{bi:X2}";
        }
        
        public static string DescribeColorMode(ColorMode colorMode)
        {
            switch (colorMode)
            {
                case ColorMode.HS:
                    return "Hue & Saturation";
                
                case ColorMode.Temp:
                    return "White Temperature";
                
                case ColorMode.RGB:
                    return "Red Green Blue";
                
                case ColorMode.XY:
                    return "X/Y";
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// Convert the chrominance to the nearest equivalent HS
        /// </summary>
        /// <returns></returns>

        public ColorHS ToHS()
        {
            GetRGB(out var r, out var g, out var b);

            //  See https://www.cs.rit.edu/~ncs/color/t_convert.html

            float min = MathF.Min(r, MathF.Min(g, b));
            float max = MathF.Max(r, MathF.Max(g, b));

            var delta = max - min;

            float h, s;

            if (max == 0)
            {
                // r = g = b = 0		// s = 0, v is undefined
                s = 0;
                h = 0;
            }
            else
            {
                s = delta / max; // s

                if (r == max)
                    h = (g - b) / delta; // between yellow & magenta
                else if (g == max)
                    h = 2 + (b - r) / delta; // between cyan & yellow
                else
                    h = 4 + (r - g) / delta; // between magenta & cyan

                if (h < 0)
                    h += 6;
            }

            return new ColorHS(h / 6, s, brightness);
        }

        public ColorRGB ToRGB()
        {
            GetRGB(out var r, out var g, out var b);
            return new ColorRGB(r, g, b, brightness);
        }

        public ColorTemp ToTemp()
        {
            throw new NotImplementedException();
        }

        public ColorXY ToXY()
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Clone this chrominance (including color mode) with the given brightness. 
        /// </summary>
        /// <param name="brightness"></param>
        /// <returns></returns>

        public abstract ColorValue WithBrightness(float brightness);
          
        /// <summary>
        /// Helper to handle the "both brightness 0" case in derived Equals implementations
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        
        protected bool AreBothOff(ColorValue? other)
        {
            if (other == null)
                return false;

            return brightness == 0 && other.brightness == 0;
        }
    }
}