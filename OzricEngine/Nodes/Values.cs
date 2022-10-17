using System.Collections.Generic;
using OzricEngine.Values;

namespace OzricEngine.Nodes;

public class Values: Dictionary<string, Value>
{
    public Values()
    {
    }

    public Values(IEnumerable<(string, Value)> attributes)
    {
        foreach (var kv in attributes)
            Add(kv.Item1, kv.Item2);
    }
}