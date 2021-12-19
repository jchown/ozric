using System;
using System.Collections.Generic;
using System.Text.Json;
using OzricEngine.ext;

namespace OzricEngine.logic
{
    public class DayPhases: Node
    {
        public enum SunPhase
        {
            Dawn, Dusk, Rising, Setting, Noon, Midnight
        }

        public class PhaseStart
        {
            public SunPhase start;
            public int startOffsetSeconds;
            public readonly Dictionary<string, object> values;

            public PhaseStart(Dictionary<string, object> values, SunPhase start, int startOffsetSeconds = 0)
            {
                this.values = values;
                this.start = start;
                this.startOffsetSeconds = startOffsetSeconds;
            }
            
            /// <summary>
            /// Return the time this phase starts. Always in today's timeframe.
            /// </summary>
            /// <param name="now">The current day.</param>
            /// <param name="sunAttributes">The attributes from the HA sun state, see https://www.home-assistant.io/integrations/sun/</param>
            /// <returns></returns>

            public DateTime GetStartTime(DateTime now, Dictionary<string, object> sunAttributes)
            {
                var attributeName = GetStartTimeAttribute();
                var dateTime = sunAttributes.Get(attributeName) as DateTime? ?? throw new Exception($"Unknown sun attribute '{attributeName}', expected one of {sunAttributes.Keys.Join(",")}");
                dateTime = dateTime.AddSeconds(startOffsetSeconds);
                return dateTime.SetDayOfYear(now.DayOfYear);
            }

            private string GetStartTimeAttribute()
            {
                return start switch
                {
                    SunPhase.Dawn => "next_dawn",
                    SunPhase.Dusk => "next_dusk",
                    SunPhase.Rising => "next_rising",
                    SunPhase.Setting => "next_setting",
                    SunPhase.Noon => "next_noon",
                    SunPhase.Midnight => "next_midnight",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public readonly List<PhaseStart> phases = new List<PhaseStart>();
            
        public DayPhases(string id) : base(id, null, null)
        {
            description = "Uses the time of day to determine the values of output";
        }
         
        public override void OnInit(Home home)
        {
            CalculateValues(home);            
        }

        public override void OnUpdate(Home home)
        {
            CalculateValues(home);            
        }

        private void CalculateValues(Home home)
        {
            if (phases.Count < 2)
            {
                return;
            }
            
            //  Figure out what phase are we in

            var sun = home.Get("sun.sun");
            var now = home.GetTime();

            int i = 0;
            var startTime = phases[0].GetStartTime(now, sun.attributes);
            do
            {
                var endTime = phases[1].GetStartTime(now, sun.attributes);

                if (startTime > endTime)
                    startTime = startTime.AddDays(-1);

                if (now >= startTime && now < endTime)
                    break;

                startTime = endTime;
                i++;

            } while (i < phases.Count - 2);

            var currentPhase = phases[i];

            foreach (var output in currentPhase.values)
            {
                SetOutputValue(output.Key, output.Value);
            }
        }
    }
}