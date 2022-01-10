using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OzricEngine.ext;
using OzricEngine.logic;

namespace OzricEngine
{
    public class EntityState: OzricObject
    {
        public string entity_id { get; set; }
        public string state { get; set; }
        public Attributes attributes { get; set; }
        public DateTime last_changed { get; set; }
        public DateTime last_updated { get; set; }
        public StateContext context { get; set; }

        public DateTime? lastUpdatedByOzric;
        public DateTime? lastUpdatedByOther;

        [JsonIgnore]
        public override string Name => (attributes.Get("friendly_name") as string ?? entity_id).Trim();

        [JsonIgnore]
        public LightAttributes LightAttributes => JsonSerializer.Deserialize<LightAttributes>(JsonSerializer.Serialize(attributes));

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
        
        /// <summary>
        /// If someone else has updated this entity, we probably shouldn't mess with it for a while. 
        /// </summary>
        /// <param name="secondsToAllowOverrideByOthers"></param>
        /// <returns></returns>

        public bool IsOverridden(DateTime now, int secondsToAllowOverrideByOthers)
        {
            return lastUpdatedByOther != null && (now - lastUpdatedByOther.Value).TotalSeconds < secondsToAllowOverrideByOthers;
        }
        
        /// <summary>
        /// We would like to know if an event was (probably) due to something we did.
        /// </summary>
        /// <param name="now"></param>
        /// <param name="secondsRecent"></param>
        /// <returns></returns>
        
        public bool WasRecentlyUpdatedByOzric(DateTime now, int secondsRecent)
        {
            return lastUpdatedByOzric != null && (now - lastUpdatedByOzric.Value).TotalSeconds < secondsRecent;
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