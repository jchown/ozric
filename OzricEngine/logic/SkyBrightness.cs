using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    public class SkyBrightness: Node
    {
        private const string ID = "sky-brightness";
        
        public const string sun = "sun";
        public const string clouds = "clouds";
        public const string brightness = "brightness";

        public SkyBrightness() : base(ID, null, new List<Pin> { new Pin(sun, new Scalar()), new Pin(clouds, new Scalar()), new Pin(brightness, new Scalar())})
        {
            description = "Combines dawn & dusk times with the current weather to determine the overall light level. 1 = bright sunshine, 0 = darkness";
        }
         
        public override Task OnInit(Engine engine)
        {
            CalculateValue(engine);
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Engine engine)
        {
            CalculateValue(engine);
            return Task.CompletedTask;
        }

        private void CalculateValue(Engine engine)
        {
            var sunLevel = GetSunLevel(engine.home);
            var cloudLevel = GetCloudLevel(engine);

            SetOutputValue(sun, new Scalar(sunLevel));
            SetOutputValue(clouds, new Scalar(cloudLevel));
            SetOutputValue(brightness, new Scalar(sunLevel - (cloudLevel * 0.8f)));
        }

        private float GetSunLevel(Home home)
        {
            //  {"next_dawn": "2021-11-30T07:21:25.459551+00:00", "next_dusk": "2021-11-30T16:39:36.918701+00:00", "next_midnight": "2021-11-30T00:00:43+00:00", "next_noon": "2021-11-30T12:00:31+00:00", "next_rising": "2021-11-30T08:03:33.515882+00:00", "next_setting": "2021-11-30T15:57:30.359979+00:00", "elevation": -26.88, "azimuth": 269.45, "rising": false, "friendly_name": "Sun"}
            
            var sun = home.Get("sun.sun");
            
            //  Find the next event to figure out where we are in the sun's cycle

            List<Tuple<DateTime, string>> events = new List<Tuple<DateTime, string>>
            {
                ParseTime(sun, "next_dawn"),
                ParseTime(sun, "next_dusk"),
                ParseTime(sun, "next_rising"),
                ParseTime(sun, "next_setting")
            };
            
            events.Sort((a,b) => a.Item1.CompareTo(b.Item1));

            switch (events[0].Item2)
            {
                case "next_dawn":
                {
                    return 0;
                }
                case "next_setting":
                {
                    return 1;
                }
                case "next_rising":
                {
                    return TimeBetween(home, events[0].Item1, events[3].Item1.Subtract(TimeSpan.FromDays(1)));
                }
                case "next_dusk":
                {
                    return 1 - TimeBetween(home, events[0].Item1, events[3].Item1.Subtract(TimeSpan.FromDays(1)));
                }
            }
            
            return 0.5f;
        }
        
        /// <summary>
        /// The time is between rising/dawn or dusk/setting, calculate how far we are in the range, 0-1
        /// </summary>
        /// <param name="home"></param>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <returns></returns>

        private float TimeBetween(Home home, DateTime to, DateTime from)
        {
            var now = home.GetTime();
            
            var timeBefore = (now - from).TotalSeconds;
            if (timeBefore <= 0)
                return 0;
            
            var timeAfter = (to - now).TotalSeconds;
            if (timeAfter <= 0)
                return 1;
            
            return (float) (timeBefore / (timeBefore + timeAfter));
        }

        private float GetCloudLevel(Engine engine)
        {
            var weather = engine.home.Get("weather.home");
            if (weather == null)
            {
                engine.Log("No weather state found");
                return 0;
            }

            switch (weather.state)
            {
                case "sunny":
                case "clear-night":
                case "windy":
                {
                    return 0;
                }

                case "windy-variant":
                case "partlycloudy":
                {
                    return 0.25f;
                }

                case "snowy":
                case "rainy":
                case "cloudy":
                {
                    return 0.5f;
                }
                
                case "fog":
                case "hail":
                case "lightning":
                case "lightning-rainy":
                case "snowy-rainy":
                case "pouring":
                case "exceptional":
                {
                    return 1;
                }
                
                default:
                {
                    engine.Log($"Unknown weather state: '{weather.state}'");
                    return 0.5f;
                }
            }
        }
        
        private Tuple<DateTime, string> ParseTime(State sun, string attribute)
        {
            var timestampString = sun.attributes[attribute].ToString();
            return Tuple.Create(DateTime.Parse(timestampString), attribute);
        }
    }
}