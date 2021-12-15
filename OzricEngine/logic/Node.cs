using System;
using System.Collections.Generic;
using System.Linq;
using OzricEngine.ext;

namespace OzricEngine.logic
{
    public abstract class Node
    {
        public string id  { get; set; }
        public string description { get; protected set; }
        public List<Pin> inputs { get; protected set; }
        public List<Pin> outputs { get; protected set; }

        protected Node(string id, List<Pin> inputs, List<Pin> outputs)
        {
            this.id = id;
            this.inputs = inputs ?? new List<Pin>();
            this.outputs = outputs ?? new List<Pin>();
        }
        
        public abstract void OnInit(Home home);
        
        public abstract void OnUpdate(Home home);
        
        internal void SetOutputValue(string name, object value)
        {
            var output = GetOutput(name) ?? throw new Exception($"Unknown output {name}");
            output.SetValue(value);
        }

        internal void SetInputValue(string name, object value)
        {
            var input = GetInput(name) ?? throw new Exception($"Unknown input {name} in {id}, possible values [{inputs.Select(i => i.name).Join(",")}]");
            input.SetValue(value);
        }

        public object GetOutputValue(string name)
        {
            return GetOutput(name)?.value ?? throw new Exception($"No output called {name}");
        }

        public Scalar GetOutputScalar(string name)
        {
            var output = GetOutputValue(name);
            return output as Scalar ?? throw new Exception($"Output {name} is a {output.GetType().Name}, not a {nameof(Scalar)}");
        }

        public Colour GetOutputColour(string name)
        {
            var output = GetOutputValue(name);
            return output as Colour ?? throw new Exception($"Output {name} is a {output.GetType().Name}, not a {nameof(Colour)}");
        }

        public OnOff GetOutputOnOff(string name)
        {
            var output = GetOutputValue(name);
            return output as OnOff ?? throw new Exception($"Output {name} is a {output.GetType().Name}, not a {nameof(OnOff)}");
        }

        private Pin GetOutput(string name)
        {
            return outputs.Find(o => o.name == name);
        }

        private Pin GetInput(string name)
        {
            return inputs.Find(o => o.name == name);
        }
    }
}