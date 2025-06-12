using System.Collections.Generic;

namespace OzricEngine.messages;

[ManualSubType]
public class ServerConfigEntityList: ServerResponse
{
    public List<EntityConfig> result { get; set; }
}