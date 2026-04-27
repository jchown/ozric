using Ozric.Engine.Messages;

namespace Ozric.Engine
{
    [TypeKey("hacs/stage")]
    public class EventHACSStage: Event
    {
        public Attributes data { get; set; }
    }
}