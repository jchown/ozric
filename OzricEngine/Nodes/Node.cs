using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OzricEngine.ext;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

public abstract class Node : OzricObject, IGraphObject, IEquatable<Node>
{
    [JsonIgnore] public override string Name => $"{GetType().Name}.{id}";

    public abstract NodeType nodeType { get; }

    public string id { get; set; }
    public List<Pin> inputs;
    public List<Pin> outputs;

    protected Node(string id, List<Pin>? inputs, List<Pin>? outputs)
    {
        this.id = id;
        this.inputs = inputs ?? new List<Pin>();
        this.outputs = outputs ?? new List<Pin>();
    }
    
    public virtual bool IsReady()
    {
        return inputs.All(input => input.value != null);
    }

    public abstract Task OnInit(Context context);

    public abstract Task OnUpdate(Context context);

    public void AddInput(string name, ValueType type)
    {
        if (HasPin(name))
            throw new Exception($"Cannot have more than one pin called {name}");
        
        inputs.Add(new Pin(name, type));
    }

    public void AddOutput(string name, ValueType type)
    {
        if (HasPin(name))
            throw new Exception($"Cannot have more than one pin called {name}");
        
        outputs.Add(new Pin(name, type));
    }

    private bool HasPin(string name)
    {
        return HasInput(name) || HasOutput(name);
    }

    public bool HasInput(string name)
    {
        return inputs.Any(i => i.name == name);
    }

    public bool HasInputValue(string name)
    {
        return inputs.FirstOrDefault(i => i.name == name)?.value != null;
    }

    public bool HasOutput(string name)
    {
        return outputs.Any(o => o.name == name);
    }
    
    public Pin GetOutput(string name)
    {
        return outputs.FirstOrDefault(o => o.name == name) ?? throw new Exception($"No output named {name} in {id}, possible values [{outputs.Select(o => o.name).Join(",")}]");
    }

    public int GetInputIndex(string name)
    {
        return inputs.FindIndex(o => o.name == name);
    }

    public int GetOutputIndex(string name)
    {
        return outputs.FindIndex(o => o.name == name);
    }

    protected Pin GetInput(string name)
    {
        return inputs.FirstOrDefault(o => o.name == name) ?? throw new Exception($"No input named {name} in {id}, possible values [{inputs.Select(i => i.name).Join(",")}]");
    }

    protected T GetInputValue<T>(string name) where T : class
    {
        var pin = GetInput(name);
        return pin.value as T ?? throw new Exception($"Input {name} was not a {typeof(T).Name}, was {pin.value?.GetType().Name}");
    }

    public T GetOutputValue<T>(string name) where T : class
    {
        var pin = GetOutput(name);
        return pin.value as T ?? throw new Exception($"Output {name} was not a {typeof(T).Name}, was {pin.value?.GetType().Name}");
    }

    public void SetOutputValue(string name, Value value, Context context)
    {
        var output = GetOutput(name);
        if (output.value != value)
        {
            Log(LogLevel.Info, "{0} = {1}", name, value);
            context.pinChanged?.Invoke(id, output.name, value);
        }

        output.SetValue(value);
    }

    public void SetInputValue(string name, Value value, Context context)
    {
        var input = GetInput(name);
        if (input.value != value)
        {
            context.pinChanged?.Invoke(id, input.name, value);
        }

        input.SetValue(value);
    }

    /// <summary>
    /// Set an input or an output directly, without triggering any change notification
    /// </summary>
    /// <param name="pinName"></param>
    /// <param name="value"></param>
    public void SetPinValue(string name, Value value)
    {
        var pin = inputs.FirstOrDefault(i => i.name == name) ?? 
                  outputs.FirstOrDefault(o => o.name == name) ?? 
                  throw new Exception($"Pin {name} not found");
        
        pin.SetValue(value);
    }
    
    public PropertyInfo GetProperty(string name)
    {
        return GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception($"Property {name} not found in {id} ({GetType().Name})");
    }

    #region Comparison
    public bool Equals(Node? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return nodeType == other.nodeType && id == other.id && Equals(inputs, other.inputs) && Equals(outputs, other.outputs);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Node)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)nodeType, id, inputs, outputs);
    }
    #endregion
}
