using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OzricEngine.ext;

namespace OzricEngine.logic
{
    public abstract class Node: OzricObject
    {
        public override string Name => id;

        public string id  { get; }
        public string description { get; protected set; }
        public List<Pin> inputs { get; }
        public List<Pin> outputs { get; }

        protected Node(string id, List<Pin> inputs, List<Pin> outputs)
        {
            this.id = id;
            this.inputs = inputs ?? new List<Pin>();
            this.outputs = outputs ?? new List<Pin>();
        }

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
            return GetInput(name) != null;
        }

        public bool HasOutput(string name)
        {
            return GetOutput(name) != null;
        }

        private Pin GetOutput(string name)
        {
            return outputs.Find(o => o.name == name);
        }

        protected Pin GetInput(string name)
        {
            return inputs.Find(o => o.name == name);
        }

        public abstract Task OnInit(Engine engine);
        
        public abstract Task OnUpdate(Engine engine);
        
        internal void SetOutputValue(string name, Value value)
        {
            var output = GetOutput(name) ?? throw new Exception($"Unknown output {name}");
            output.SetValue(value);
        }

        public void SetInputValue(string name, Value value)
        {
            var input = GetInput(name) ?? throw new Exception($"Unknown input {name} in {id}, possible values [{inputs.Select(i => i.name).Join(",")}]");
            input.SetValue(value);
        }

        public Value GetOutputValue(string name)
        {
            return GetOutput(name)?.value ?? throw new Exception($"No output called {name}, possible values [{outputs.Select(o => o.name).Join(",")}]");
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

        public OnOff GetOutputOnOff(string name)
        {
            var output = GetOutputValue(name);
            return output as OnOff ?? throw new Exception($"Output {name} is a {output.GetType().Name}, not a {nameof(OnOff)}");
        }
    }
}