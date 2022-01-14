using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    /// <summary>
    /// Base class for all colors. All colors have a single <see cref="ValueType" />, 
    /// and use <see cref="ColorType"/> to distinguish them.
    /// </summary>

    public abstract class ColorValue: Value
    {
        public override ValueType ValueType => ValueType.Color;

        [JsonPropertyName("color-type")]
        public abstract ColorType ColorType { get; }

        public float brightness { get; protected set; }

        [JsonIgnore]
        public abstract float luminance { get; }

        protected ColorValue()
        {
        }

        protected ColorValue(float brightness)
        {
            this.brightness = brightness;
        }

        private static readonly Dictionary<ColorType, Json.CreateObject<ColorValue>> creators = new Dictionary<ColorType, Json.CreateObject<ColorValue>>
        {
            { ColorType.Temp, ColorTemp.ReadFromJSON },
            { ColorType.HS, ColorHS.ReadFromJSON },
            { ColorType.RGB, ColorRGB.ReadFromJSON },
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

        public virtual void GetRGB(out float r, out float g, out float b)
        {
            throw new NotImplementedException();
        }

        public override void WriteAsJSON(Utf8JsonWriter writer)
        {
            writer.WriteString("color-type", ColorType.ToString());
            writer.WriteNumber("brightness", brightness);
        }
    }
}