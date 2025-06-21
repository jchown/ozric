using System.Collections.Generic;

namespace Ozric.Engine.Graph;

public interface IGraph
{
    public IList<Node> GetConnectedNodes(string nodeId);
}