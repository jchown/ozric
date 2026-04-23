using Ozric.Engine.Graph;
using OzricEngine.Nodes;

namespace OzricEngine
{
    /// <summary>
    /// Node deserializer
    /// </summary>
    public class JsonConverterNode: JsonConverterBase<GraphNode>
    {
        public JsonConverterNode() : base("node-type")
        {
        }
    }
}