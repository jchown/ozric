using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using OzricEngine.ext;
using OzricEngine.Nodes;

namespace OzricEngine
{
    public class EntityState: OzricObject
    {
        public string entity_id { get; set; }
        public string state { get; set; }
        public Attributes attributes { get; set; }
        public DateTime last_changed { get; set; }
        public DateTime last_updated { get; set; }
        public MessageContext context { get; set; }

        [JsonIgnore]
        public DateTime? lastUpdatedByOther;

        public Category GetCategory()
        {
            switch (entity_id.Substring(0, entity_id.IndexOf('.')))
            {
                case "light":
                    return Category.Light;
                case "switch":
                    return Category.Switch;
                case "sensor":
                    return Category.ModeSensor;
                case "binary_sensor":
                    return Category.Sensor;
                case "media_player":
                    return Category.MediaPlayer;
                case "person":
                    return Category.Person;
                default:
                    return Category.Unknown;
            }
        }

        [JsonIgnore]
        public override string Name => (attributes.Get("friendly_name") as string ?? entity_id).Trim();

        [JsonIgnore]
        public LightAttributes LightAttributes => Json.Deserialize<LightAttributes>(Json.Serialize(attributes));

        public override string ToString()
        {
            return Json.Serialize(this);
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

        public float GetNumSecondsSinceOverride(DateTime now)
        {
            if (lastUpdatedByOther == null)
                return 999999999;
            
            return (float)(now - lastUpdatedByOther.Value).TotalSeconds;
        }

        public void LogLightState(LogLevel level = LogLevel.Info)
        {
            if (state == "on")
            {
                if (attributes.ContainsKey("color_mode"))
                {
                    string colorMode = attributes["color_mode"].ToString()!;
                    string colorKey = colorMode switch
                    {
                        "color_temp" => "color_temp",
                        _ => $"{colorMode}_color"
                    };

                    Log(level, "{0}: on, brightness = {1}, {2} = {3}", entity_id, attributes["brightness"], colorMode, attributes[colorKey]);
                }
                else
                {
                    Log(level, "{0}: on, brightness = {1}", entity_id, attributes["brightness"]);
                }
            }
            else
            {
                Log(level, "{0}: {1}", entity_id, state);
            }
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
        public int brightness;
        
        public string color_mode;
        public string[] supported_color_modes;

        public int? color_temp;
        public float[]? hs_color ;
        public int[]? rgb_color ;
        public int[]? rgbw_color ;
        public int[]? rgbww_color ;
        public float[]? xy_color ;
        
        public string effect ;
        public string[] effect_list ;
        public string friendly_name ;

        public int min_mireds;
        public int max_mireds;
        
        public int supported_features;
    }
}