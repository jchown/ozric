using System;

namespace OzricEngine.logic
{
    public class SkyBrightness: Scalar
    {
        private const string ID = "sky-brightness";

        public SkyBrightness() : base(ID)
        {
            description = "Combines dawn & dusk times with the current weather to determine the overall light level. 1 = bright sunshine, 0 = darkness";
        }
         
        public override void OnInit(Home home)
        {
            CalculateValue(home);            
        }

        public override void OnUpdate(Home home)
        {
            CalculateValue(home);            
        }

        private void CalculateValue(Home home)
        {
            var sun = home.Get("sun.sun");
            var dawn = ParseTime(sun, "next_dawn");
            var dusk = ParseTime(sun, "next_dusk");
            var rising = ParseTime(sun, "next_rising");
            var setting = ParseTime(sun, "next_setting");
        }

        private DateTime ParseTime(State sun, string attribute)
        {
            var timestampString = sun.attributes[attribute].ToString();
            return DateTime.Parse(timestampString);
        }
    }
}