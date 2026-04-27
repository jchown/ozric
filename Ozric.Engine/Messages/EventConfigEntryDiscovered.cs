using Ozric.Engine.Messages;

namespace Ozric.Engine
{
    [TypeKey("config_entry_discovered")]
    public class ConfigEntryDicovered: Event
    {
        public Attributes data { get; set; }
    }
}