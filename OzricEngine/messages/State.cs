using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OzricEngine.ext;

namespace OzricEngine
{
    public class State
    {
        public string entity_id { get; set; }
        public string state { get; set; }
        [JsonConverter(typeof(JsonConverterDictionaryObjectValues))]
        public Dictionary<string, object> attributes { get; set; }
        public DateTime last_changed { get; set; }
        public DateTime last_updated { get; set; }
        public StateContext context { get; set; }

        [JsonIgnore]
        public string Name => (attributes.Get("friendly_name") as string ?? entity_id).Trim();

        [JsonIgnore]
        public LightAttributes LightAttributes => JsonSerializer.Deserialize<LightAttributes>(JsonSerializer.Serialize(attributes));

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }


    enum SupportedFeatures
    {
        BRIGHTNESS = 1,
        COLOR_TEMP = 2,
        EFFECT = 4,
        FLASH = 8,
        COLOR = 16,
        TRANSITION = 32,
        WHITE_VALUE = 128
    }

    /// <summary>
    /// See https://developers.home-assistant.io/docs/core/entity/light/
    /// </summary>
    public class LightAttributes
    {
        public int brightness { get; set; }
        
        public string color_mode { get; set; }
        public string[] supported_color_modes { get; set; }

        public int color_temp { get; set; }
        public float[] hs_color { get; set; }
        public int[] rgb_color { get; set; }
        public int[] rgbw_color { get; set; }
        public int[] rgbww_color { get; set; }
        public float[] xy_color { get; set; }
        
        public string effect { get; set; }
        public string[] effect_list { get; set; }
        public string friendly_name { get; set; }

        public int min_mireds  { get; set; }
        public int max_mireds { get; set; }
        
        public int supported_features { get; set; }
    }
}