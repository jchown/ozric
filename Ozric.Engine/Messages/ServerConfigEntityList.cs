using System.Collections.Generic;
using Ozric.Engine.Model;

namespace OzricEngine.messages;

[ManualSubType]
public class ServerConfigEntityList: ServerResponse
{
    public List<EntityConfig> result { get; set; }
}