using System.Collections.Generic;
using Ozric.Engine.Model;

namespace OzricEngine.messages;

[ManualSubType]
public class ServerConfigAreaList: ServerResponse
{
    public List<AreaConfig> result { get; set; }
}