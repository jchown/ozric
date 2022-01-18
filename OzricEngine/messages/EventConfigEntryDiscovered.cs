namespace OzricEngine
{
    [TypeKey("config_entry_discovered")]
    public class ConfigEntryDicovered: Event
    {
        public Attributes data { get; set; }
    }
}