using System.Collections.Generic;
using Ozric.Engine.Model;

namespace Ozric.Engine.Messages;

[ManualSubType]
public class ServerConfigEntityList: ServerResponse
{
    public List<EntityConfig> result { get; set; }
}