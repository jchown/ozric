using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    /// <summary>
    /// Base class for all colors. All colors have a single <see cref="ValueType" />, 
    /// and use <see cref="ColorMode"/> to distinguish them.
    /// </summary>

    public abstract class ColorValue: Value
    {
        public override ValueType ValueType => ValueType.Color;

        public abstract ColorMode ColorMode { get; }

        /// <summary>
        /// 0-1 
        /// </summary>
        public float brightness { get; set; }

        [JsonIgnore]
        public abstract float luminance { get; }

        protected ColorValue()
        {
        }

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
        /// <exception cref="NotImplementedException"></exception>
        public virtual void GetRGB(out float r, out float g, out float b)
        {
            throw new NotImplementedException();
        }

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
        /// Convert the primary color component (ignoring brightness) to the nearest equivalent HS
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
    }
}