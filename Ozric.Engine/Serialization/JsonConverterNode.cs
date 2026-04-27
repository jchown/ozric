using Ozric.Engine.Graph;

namespace Ozric.Engine
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