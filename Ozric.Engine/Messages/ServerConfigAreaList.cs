using System.Collections.Generic;
using Ozric.Engine.Model;

namespace Ozric.Engine.Messages;

[ManualSubType]
public class ServerConfigAreaList: ServerResponse
{
    public List<AreaConfig> result { get; set; }
}