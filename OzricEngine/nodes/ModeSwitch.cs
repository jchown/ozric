using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    /// <summary>
    /// Choose from a number of values based on the value of a Mode input
    /// </summary>
    [TypeKey(NodeType.ModeSwitch)]
    public class ModeSwitch: Node
    {
        [JsonPropertyName("node-type")]
        public override NodeType nodeType => NodeType.ModeSwitch;

        public class ModeValues
        {
            public ModeValues(Mode mode, Values values)
            {
                this.mode = mode;
                this.values = values;
            }

            public Mode mode { get; }
            public Values values { get; }
        }
        
        public List<ModeValues> modeValues { get; }
        
        public ModeSwitch(string id): base(id, new List<Pin> { new("mode", ValueType.Mode) }, null)
        {
            modeValues = new List<ModeValues>();
        }
        
        public void AddModeValues(string mode, params ValueTuple<string, Value>[] attributes)
        {
            foreach (var (key, value) in attributes)
            {
                if (!HasOutput(key))
                    throw new Exception($"No output named {key}");

                var expectedType = GetOutput(key).type;
                if (expectedType != value.ValueType)
                    throw new Exception($"Expected value of type {expectedType} for {key}, but was {value.ValueType}");
            }
            
            modeValues.Add(new ModeValues(new Mode(mode), new Values(attributes)));
        }

        public override Task OnInit(Context context)
        {
            UpdateValue();
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Context context)
        {
            UpdateValue();
            return Task.CompletedTask;
        }

        private void UpdateValue()
        {
            var currentMode = GetInput("mode").value as Mode ?? throw new Exception("No 'mode' found");

            foreach (var modeValue in modeValues)
            {
                if (modeValue.mode == currentMode)
                {
                    foreach (var value in modeValue.values)
                        SetOutputValue(value.Key, value.Value);
                    
                    return;
                }
            }

            throw new Exception($"Unknown mode {currentMode}");
        }
    }

    public class Values: Dictionary<string, Value>
    {
        public Values((string, Value)[] attributes)
        {
            foreach (var kv in attributes)
                Add(kv.Item1, kv.Item2);
        }
    }
}