using System.Text.Json.Serialization;

namespace OzricEngine.logic
{
    public abstract class ColorValue: Value
    {
        public float brightness { get; protected set; }

        [JsonIgnore]
        public abstract float luminance { get; set; }

        protected ColorValue()
        {
            brightness = 1;
        }
    }
}