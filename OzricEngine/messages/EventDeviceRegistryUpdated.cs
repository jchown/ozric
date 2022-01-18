namespace OzricEngine
{
    [TypeKey("device_registry_updated")]
    public class DeviceRegistryUpdated: Event
    {
        public Attributes data { get; set; }
    }
}