using System.Collections.Generic;
using Ozric.Engine.Model;

namespace Ozric.Engine.Messages;

[ManualSubType]
public class ServerConfigDeviceList: ServerResponse
{
    public List<DeviceConfig> result { get; set; }
}