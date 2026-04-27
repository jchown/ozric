using System;
using System.Text.Json.Serialization;
using Ozric.Engine.Extensions;
using Ozric.Engine.Graph;
using Ozric.Engine.Messages;
using Ozric.Engine.Nodes;
using Ozric.Engine.Utils;

namespace Ozric.Engine.Model
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
            return CategoryModelMappings.FromEntityID(entity_id);
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
}