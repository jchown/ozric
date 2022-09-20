using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OzricEngine.ext;
using OzricEngine.Values;
using Boolean = OzricEngine.Values.Boolean;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

public abstract class Node: OzricObject, IGraphObject, IEquatable<Node>
{
    [JsonIgnore]
    public override string Name => $"{GetType().Name}.{id}";

    public abstract NodeType nodeType { get; }
        
    public string id { get; set; }
    public List<Pin> inputs { get; set; }
    public List<Pin> outputs { get; set; }

    protected Node(string id, List<Pin>? inputs, List<Pin>? outputs)
    {
        this.id = id;
        this.inputs = inputs ?? new List<Pin>();
        this.outputs = outputs ?? new List<Pin>();
    }
        
    public abstract Task OnInit(Context context);
        
    public abstract Task OnUpdate(Context context);

    public void AddInput(string name, ValueType type)
    {
        inputs.Add(new Pin(name, type));
    }

    public void AddOutput(string name, ValueType type)
    {
        outputs.Add(new Pin(name, type));
    }

    public bool HasInput(string name)
    {
        return inputs.Any(i => i.name == name);
    }

    public bool HasOutput(string name)
    {
        return outputs.Any(o => o.name == name);
    }

    protected Pin GetOutput(string name)
    {
        return outputs.FirstOrDefault(o => o.name == name) ?? throw new Exception($"No output named {name} in {id}, possible values [{outputs.Select(o => o.name).Join(",")}]");
    }
        
    public int GetOutputIndex(string name)
    {
        return outputs.FindIndex(o => o.name == name);
    }

    protected Pin GetInput(string name)
    {
        return inputs.FirstOrDefault(o => o.name == name) ?? throw new Exception($"No input named {name} in {id}, possible values [{inputs.Select(i => i.name).Join(",")}]");
    }
        
    public int GetInputIndex(string name)
    {
        return inputs.FindIndex(o => o.name == name);
    }

    internal void SetOutputValue(string name, Value value)
    {
        var output = GetOutput(name);
        if (output.value != value)
            Log(LogLevel.Info, "{0} = {1}", name, value);
            
        output.SetValue(value);
    }

    public void SetInputValue(string name, Value value)
    {
        var input = GetInput(name);
        input.SetValue(value);
    }

    public Value GetOutputValue(string name)
    {
        return GetOutput(name).value!;
    }

    public Scalar GetOutputScalar(string name)
    {
        var output = GetOutputValue(name);
        return output as Scalar ?? throw new Exception($"Output {name} is a {output.GetType().Name}, not a {nameof(Scalar)}");
    }

    public ColorValue GetOutputColor(string name)
    {
        var output = GetOutputValue(name);
        return output as ColorValue ?? throw new Exception($"Output {name} is a {output.GetType().Name}, not a {nameof(ColorRGB)}");
    }

    public ColorValue GetInputColor(string name)
    {
        var output = GetOutputValue(name);
        return output as ColorValue ?? throw new Exception($"Output {name} is a {output.GetType().Name}, not a {nameof(ColorValue)}");
    }

    public Mode GetInputMode(string name)
    {
        var output = GetOutputValue(name);
        return output as Mode ?? throw new Exception($"Output {name} is a {output.GetType().Name}, not a {nameof(Mode)}");
    }

    public Boolean GetOutputOnOff(string name)
    {
        var output = GetOutputValue(name);
        return output as Boolean ?? throw new Exception($"Output {name} is a {output.GetType().Name}, not a {nameof(Boolean)}");
    }
        
    public PropertyInfo GetProperty(string name)
    {
        return GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception($"Property {name} not found");
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