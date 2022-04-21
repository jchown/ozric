using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OzricEngine.logic
{
    /// <summary>
    /// Choose from two values based on the state of an OnOff input
    /// </summary>
    [TypeKey(NodeType.Switch)]
    public class Switch: Node
    {
        public override NodeType nodeType => NodeType.Switch;

        public ValueType valueType { get; set; }
        
        public const string INPUT_NAME_ON = "on";
        public const string INPUT_NAME_OFF = "off";
        public const string INPUT_NAME_SWITCH = "switch";
        public const string OUTPUT_NAME = "output";
        
        public Switch(string id, ValueType valueType): base(id, new List<Pin> { new(INPUT_NAME_ON, valueType), new(INPUT_NAME_OFF, valueType), new(INPUT_NAME_SWITCH, ValueType.OnOff) }, new List<Pin> { new(OUTPUT_NAME, valueType) })
        {
            this.valueType = valueType;
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
            var switcher = GetInput("switch").value as OnOff ?? throw new Exception("No 'switch' found");

            if (switcher.value)
            {
                SetOutputValue("output", GetInput("on").value);
            }
            else
            {
                SetOutputValue("output", GetInput("off").value);
            }
        }
    }
}