using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OzricEngine
{
    public class EventCallServiceServiceData
    {
        public int brightness { get; set; }

        public int? color_temp { get; set; }
        public float[] hs_color { get; set; }
        public int[] rgb_color { get; set; }
        public int[] rgbw_color { get; set; }
        public int[] rgbww_color { get; set; }
        public float[] xy_color { get; set; }

        [JsonConverter(typeof(JsonConverterEntityID))]
        public List<string> entity_id { get; set; }   // Either a string[] or string
    }
}