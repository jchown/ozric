namespace OzricEngine
{
    public class EventEntityRegistryUpdatedData
    {
        public string action { get; set; }
        public string entity_id { get; set; }
        public Attributes changes { get; set; }
    }
}