using System.Collections.Generic;

namespace Ozric.Engine.Graph;

public interface IGraph
{
    public IList<GraphNode> GetConnectedNodes(string nodeId);
}