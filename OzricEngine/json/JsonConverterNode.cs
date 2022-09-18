using OzricEngine.Nodes;

namespace OzricEngine
{
    /// <summary>
    /// Node deserializer
    /// </summary>
    public class JsonConverterNode: JsonConverterBase<Node>
    {
        public JsonConverterNode() : base("node-type")
        {
        }
    }
}