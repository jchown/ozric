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

    private Values(Values values)
    {
        foreach (var kv in values)
            Add(kv.Key, kv.Value);
    }

    public Values Clone()
    {
        return new Values(this);
    }
    
    public Value? GetOrNull(string key)
    {
        return TryGetValue(key, out var value) ? value : null;
    }
}