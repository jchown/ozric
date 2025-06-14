using System.Collections.Generic;

namespace OzricEngine.messages;

[ManualSubType]
public class ServerConfigAreaList: ServerResponse
{
    public List<AreaConfig> result { get; set; }
}