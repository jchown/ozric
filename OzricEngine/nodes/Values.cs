using System.Collections.Generic;

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