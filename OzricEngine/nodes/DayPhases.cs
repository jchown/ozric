using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Humanizer;
using OzricEngine.ext;

namespace OzricEngine.logic
{
    /// <summary>
    /// Split the day into phases, emitting a Mode for each one.
    /// </summary>
    [TypeKey(NodeType.DayPhases)]
    public class DayPhases: Node
    {
        [JsonPropertyName("node-type")]
        public override NodeType nodeType => NodeType.DayPhases;

        public List<PhaseStart> phases { get; }
            
        public DayPhases(string id) : base(id, null, new List<Pin> { new("mode", ValueType.Mode) })
        {
            phases = new List<PhaseStart>();
        }

        [JsonConverter(typeof(JsonStringEnumConverter))] 
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
            public SunPhase start { get; }
            public int startOffsetSeconds { get; }
            public Mode mode { get; }

            public PhaseStart(Mode mode, SunPhase start, int startOffsetSeconds = 0)
            {
                this.start = start;
                this.startOffsetSeconds = startOffsetSeconds;
                this.mode = mode;
            }
            
            /// <summary>
            /// Return the time this phase starts. Always in today's timeframe.
            /// </summary>
            /// <param name="now">The current day.</param>
            /// <param name="sunAttributes">The attributes from the HA sun state, see https://www.home-assistant.io/integrations/sun/</param>
            /// <returns></returns>

            public DateTime GetStartTime(DateTime now, Attributes sunAttributes)
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

            public static PhaseStart Create(SunPhase sunPhase, int offsetSeconds, Mode mode)
            {
                return new PhaseStart(mode, sunPhase, offsetSeconds);
            }
        }

        public override Task OnInit(Context context)
        {
            CalculateValues(context);            
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Context engine)
        {
            CalculateValues(engine);
            return Task.CompletedTask;
        }

        private void CalculateValues(Context context)
        {
            if (phases.Count == 0)
                return;

            if (phases.Count == 1)
            {
                var onlyPhase = phases[0];

                Log(LogLevel.Debug, "phase can only be [{0}]", onlyPhase);

                SetOutputValue("mode", onlyPhase.mode);
                return;
            }

            //  Figure out what phase are we in

            var sun = context.engine.home.GetEntityState("sun.sun");
            var now = context.engine.home.GetTime();

            int i = 1;
            var startTime = phases[0].GetStartTime(now, sun.attributes);
            do
            {
                var endTime = phases[i % phases.Count].GetStartTime(now, sun.attributes);

                if (startTime > endTime)    // Watch for wrap-around to start of day
                {
                    if (now >= startTime && now < endTime.AddDays(1))
                        break;
                    
                    if (now >= startTime.AddDays(-1) && now < endTime)
                        break;
                }
                else
                {
                    if (now >= startTime && now < endTime)
                        break;
                }

                startTime = endTime;
                i++;

            } while (i < phases.Count);

            var currentPhase = phases[i - 1];
            var nextPhase = phases[i % phases.Count];
            
            Log(LogLevel.Debug, "phase is between {0} and {1}", currentPhase, nextPhase);

            SetOutputValue("mode", currentPhase.mode);
        }

        public void AddPhase(PhaseStart phaseStart)
        {
            phases.Add(phaseStart);
        }
    }
}