using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    /// <summary>
    /// Choose from a number of values based on the value of a Mode input
    /// </summary>
    public class ModeSwitch: Node
    {
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
        
        private readonly List<ModeValues> modeValues;
        
        public ModeSwitch(string id): base(id, new List<Pin> { new Pin("mode", ValueType.Mode) }, null)
        {
            modeValues = new List<ModeValues>();
        }

        public override Task OnInit(Engine engine)
        {
            UpdateValue();
            return Task.CompletedTask;
        }

        public override Task OnUpdate(Engine engine)
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
        
        public void AddModeValues(string mode, params ValueTuple<string, Value>[] attributes)
        {
            foreach (var attribute in attributes)
            {
                var key = attribute.Item1;
                var value = attribute.Item2;

                if (!HasOutput(key))
                    throw new Exception($"No output named {key}");

                var expectedType = GetOutput(key).type;
                if (expectedType != value.ValueType)
                    throw new Exception($"Expected value of type {expectedType} for {key}, but was {value.ValueType}");
            }
            
            modeValues.Add(new ModeValues(new Mode(mode), new Values(attributes)));
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