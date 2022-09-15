using System.Collections.Generic;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.logic;

public class Values: Dictionary<string, Value>
{
    public Values()
    {
    }

    public Values((string, Value)[] attributes)
    {
        foreach (var kv in attributes)
            Add(kv.Item1, kv.Item2);
    }
}