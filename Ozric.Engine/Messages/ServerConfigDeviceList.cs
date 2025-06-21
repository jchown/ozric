using System.Collections.Generic;
using Ozric.Engine.Model;
using Sentry.Protocol;

namespace OzricEngine.messages;

[ManualSubType]
public class ServerConfigDeviceList: ServerResponse
{
    public List<DeviceConfig> result { get; set; }
}