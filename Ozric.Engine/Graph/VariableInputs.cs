using System;
using System.Collections.Generic;
using System.Linq;
using Ozric.Engine.Graph;
using OzricEngine.Values;
using ValueType = Ozric.Engine.Values.ValueType;

namespace OzricEngine.Nodes;

/// <summary>
/// TODO: Remove and replace with a multi-value input type.
/// </summary>
public abstract class VariableInputs: Node
{
    public readonly ValueType valueType;

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
        var expected = $"input-{inputs.Count + 1}";
        if (!HasInput(expected))
            return expected;

        for (var i = 1; i <= inputs.Count; i++)
        {
            var mid = $"input-{i}";
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
        if (obj is not VariableInputs vi)
            return false;
        
        return base.Equals(obj) && valueType == vi.valueType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), valueType);
    }
    #endregion

}