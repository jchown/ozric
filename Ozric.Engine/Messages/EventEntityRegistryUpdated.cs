namespace OzricEngine
{
    [TypeKey("entity_registry_updated")]
    public class EventEntityRegistryUpdated: Event
    {
        public EventEntityRegistryUpdatedData data { get; set; }
    }
}