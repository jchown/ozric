using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

/// <summary>
/// Choose from a number of values based on the value of a Mode input
/// </summary>
[TypeKey(NodeType.ModeSwitch)]
public class ModeSwitch: Node
{
    public const string INPUT_NAME = "mode";
        
    public override NodeType nodeType => NodeType.ModeSwitch;

    public class ModeValues
    {
        public ModeValues(Mode mode, Values values)
        {
            this.mode = mode;
            this.values = values;
        }

        public Mode mode { get; set; }
        public Values values { get; set; }
    }
    
    public List<ModeValues> modeValues { get; set; }
        
    public ModeSwitch(string id): base(id, new List<Pin> { new(INPUT_NAME, ValueType.Mode) }, null)
    {
        modeValues = new List<ModeValues>();
    }
        
    public void AddModeValues(string mode, params ValueTuple<string, Value>[] attributes)
    {
        foreach (var (key, value) in attributes)
        {
            if (!HasOutput(key))
                throw new Exception($"No output named {key}");

            var expectedType = GetOutput(key)?.type;
            if (expectedType != value.ValueType)
                throw new Exception($"Expected value of type {expectedType} for {key}, but was {value.ValueType}");
        }
            
        modeValues.Add(new ModeValues(new Mode(mode), new Values(attributes)));
    }

    public override Task OnInit(Context context)
    {
        UpdateValue(context);
        return Task.CompletedTask;
    }

    public override Task OnUpdate(Context context)
    {
        UpdateValue(context);
        return Task.CompletedTask;
    }

    private void UpdateValue(Context context)
    {
        var currentMode = GetInputValue<Mode>(INPUT_NAME);

        foreach (var modeValue in modeValues)
        {
            if (modeValue.mode == currentMode)
            {
                foreach (var value in modeValue.values)
                    SetOutputValue(value.Key, value.Value, context);
                    
                return;
            }
        }

        throw new Exception($"Unknown mode {currentMode}");
    }

    public IEnumerable<string> GetKeys()
    {
        return outputs.Select(o => o.name);
    }

}