using System;
using System.Collections.Generic;

namespace OzricEngine.logic
{
    public abstract class Node
    {
        public readonly string id;
        public string description { get; protected set; }
        public List<Input> inputs { get; protected set; }
        public List<Output> outputs { get; protected set; }

        protected Node(string id, List<Input> inputs): this(id, inputs, new List<Output>())
        {
        }


        protected Node(string id, List<Output> outputs): this(id, new List<Input>(), outputs)
        {
        }

        protected Node(string id, List<Input> inputs, List<Output> outputs)
        {
            this.id = id;
            this.inputs = inputs;
            this.outputs = outputs;
        }
        
        public abstract void OnInit(Home home);
        
        public abstract void OnUpdate(Home home);

        protected void SetOutputValue(string name, Scalar scalar)
        {
            var output = GetOutput(name) ?? throw new Exception($"No output called {name}");
            
            switch (output.value)
            {
                case Scalar s:
                    s.value = scalar.value;
                    return;
                
                case Colour c:
                    c.r = c.g = c.b = scalar.value;
                    return;
                
                case OnOff onOff:
                    onOff.value = scalar.value >= 0.5f;
                    return;
                
                default:
                    throw new Exception($"Don't know how to assign Scalar to {output.GetType().Name}");
            }
        }
        
        protected void SetOutputValue(string name, Colour colour)
        {
            var output = GetOutput(name) ?? throw new Exception($"No output called {name}");
            
            switch (output.value)
            {
                case Scalar s:
                    s.value = colour.luminance;
                    return;
                
                case Colour c:
                    c.r = colour.r;
                    c.g = colour.g;
                    c.b = colour.b;
                    c.a = colour.a;
                    return;
                
                case OnOff onOff:
                    onOff.value = colour.luminance >= 0.5f;
                    return;
                
                default:
                    throw new Exception($"Don't know how to assign Colour to {output.GetType().Name}");
            }
        }
        
        protected void SetOutputValue(string name, object value)
        {
            switch (value)
            {
                case Scalar scalar:
                    SetOutputValue(name, scalar);
                    return;
                
                case Colour colour:
                    SetOutputValue(name, colour);
                    return;
                
                case OnOff onOff:
                    SetOutputValue(name, onOff);
                    return;
                
                default:
                    throw new Exception($"Don't know how to assign {value.GetType().Name}");
            }
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

        private Output GetOutput(string name)
        {
            return outputs.Find(o => o.name == name);
        }
    }
}