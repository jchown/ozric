using Ozric.Engine.Messages;

namespace Ozric.Engine
{
    [TypeKey("hacs/repository")]
    public class EventHACSRepository: Event
    {
        public Attributes data { get; set; }
    }
}