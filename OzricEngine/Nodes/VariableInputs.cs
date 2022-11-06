using System;
using System.Collections.Generic;
using System.Linq;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

public abstract class VariableInputs: Node
{
    public ValueType valueType { get; }

    public VariableInputs(string id, ValueType valueType, List<Pin> outputs): base(id, null, outputs)
    {
        this.valueType = valueType;
    }

    public Pin AddInput(string name)
    {
        var input = new Pin(name, valueType);
        inputs.Add(input);
        return input;
    }

    public Pin RemoveInput(string name)
    {
        var input = inputs.First(i => i.name == name);
        inputs.Remove(input);
        return input;
    }

    public string NextPinName()
    {
        string expected = $"input-{inputs.Count + 1}";
        if (!HasInput(expected))
            return expected;

        for (int i = 1; i <= inputs.Count; i++)
        {
            string mid = $"input-{i}";
            if (!HasInput(mid))
                return mid;
        }
        
        string unexpected = $"input-{inputs.Count + 2}";
        if (!HasInput(unexpected))
            return expected;
        
        throw new Exception(@"¯\_(ツ)_/¯");
    }

    protected IEnumerable<TValue> GetInputValues<TValue>() where TValue: Value
    {
        return inputs.Select(input => (TValue) input.value!);
    }
        
    #region Comparison
    public override bool Equals(object? obj)
    {
        if (!(obj is VariableInputs vi))
            return false;
        
        return base.Equals(obj) && valueType == vi.valueType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetHashCode(), valueType);
    }
    #endregion

}