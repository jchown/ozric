using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Humanizer;
using OzricEngine.ext;

namespace OzricEngine.logic
{
    public class DayPhases: Node
    {
        public enum SunPhase
        {
            /// <summary>
            /// The sun starts to rise. Followed by [SunPhase.Rising]
            /// </summary>
            Dawn,
            
            /// <summary>
            /// The sun has fully set.
            /// </summary>
            Dusk,
            
            /// <summary>
            /// Dawn has finished.
            /// </summary>
            Rising,
            
            /// <summary>
            /// Sunset has started. Followed by [SunPhase.Dusk]
            /// </summary>
            Setting, 
            
            /// <summary>
            /// Always 12:00 local time
            /// </summary>
            Noon,
            
            /// <summary>
            /// Always 00:00 local time
            /// </summary>
            Midnight
        }

        public class PhaseStart
        {
            public SunPhase start;
            public int startOffsetSeconds;
            public readonly Dictionary<string, Value> values;

            public PhaseStart(Dictionary<string, Value> values, SunPhase start, int startOffsetSeconds = 0)
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
                var attributeValue = sunAttributes.Get(attributeName) ?? throw new Exception($"Unknown sun attribute '{attributeName}', expected one of {sunAttributes.Keys.Join(",")}");
                DateTime dateTime;
                switch (attributeValue)
                {
                    case JsonElement je:
                    {
                        if (!je.TryGetDateTime(out dateTime))
                            throw new Exception($"Failed to parse {je} as a DateTime");
                        break;
                    }
                    case DateTime dt:
                    {
                        dateTime = dt;
                        break;
                    }

                    default:
                    {
                        throw new Exception($"Unexpected sun attribute '{attributeName}' type, expected {nameof(DateTime)} but was {attributeValue.GetType().Name}");
                    }
                }

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

            public override string ToString()
            {
                if (startOffsetSeconds == 0)
                    return $"{start}";

                var offset = TimeSpan.FromSeconds(Math.Abs(startOffsetSeconds)).Humanize();
                if (startOffsetSeconds > 0)
                {
                    return $"{start} +{offset}";
                }

                return $"{start} -{offset}";
            }

            public static PhaseStart Create(SunPhase sunPhase, int offsetSeconds, params ValueTuple<string, Value>[] attributes)
            {
                return new PhaseStart(attributes.ToDictionary(a => a.Item1, a => a.Item2), sunPhase, offsetSeconds);
            }
        }

        public readonly List<PhaseStart> phases = new List<PhaseStart>();
            
        public DayPhases(string id) : base(id, null, null)
        {
            description = "Uses the time of day to determine the values of output";
        }
         
        public override Task OnInit(Engine engine)
        {
            CalculateValues(engine);            
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Engine engine)
        {
            CalculateValues(engine);
            return Task.CompletedTask;
        }

        private void CalculateValues(Engine engine)
        {
            if (phases.Count == 0)
                return;

            if (phases.Count == 1)
            {
                var onlyPhase = phases[0];

                engine.Log($"{id}.phase can only be [{onlyPhase}]");

                foreach (var output in onlyPhase.values)
                {
                    SetOutputValue(output.Key, output.Value);
                }

                return;
            }

            //  Figure out what phase are we in

            var sun = engine.home.Get("sun.sun");
            var now = engine.home.GetTime();

            int i = 1;
            var startTime = phases[0].GetStartTime(now, sun.attributes);
            do
            {
                var endTime = phases[i % phases.Count].GetStartTime(now, sun.attributes);

                if (startTime > endTime)
                    startTime = startTime.AddDays(-1);

                if (now >= startTime && now < endTime)
                    break;

                startTime = endTime;
                i++;

            } while (i < phases.Count);

            var currentPhase = phases[i - 1];
            var nextPhase = phases[i % phases.Count];
            
            engine.Log($"{id}.phase is between [{currentPhase}] and [{nextPhase}]");

            foreach (var output in currentPhase.values)
            {
                SetOutputValue(output.Key, output.Value);
            }
        }

        public void AddPhase(PhaseStart phaseStart)
        {
            var missing = phaseStart.values.Keys.FirstOrDefault(o => !HasOutput(o));
            if (missing != null)
                throw new Exception($"{missing} is not an output of {this}");

            phases.Add(phaseStart);
        }
    }
}