using Ozric.Engine.Messages;

namespace Ozric.Engine
{
    [TypeKey("device_registry_updated")]
    public class DeviceRegistryUpdated: Event
    {
        public Attributes data { get; set; }
    }
}