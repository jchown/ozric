using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Boolean = OzricEngine.Values.Boolean;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

/// <summary>
/// Once an input value is set for a period, keep it at that value
/// </summary>
[TypeKey(NodeType.BinarySustain)]
public class BinarySustain: Node
{
    public override NodeType nodeType => NodeType.BinarySustain;

    public bool sustainValue { get; set; }
    public double sustainActivateSecs { get; set; }
    public double sustainDeactivateSecs { get; set; }

    [JsonIgnore]
    private DateTime? _valueSet;

    [JsonIgnore]
    private DateTime? _valueUnset;
        
    public const string INPUT_NAME = "in";
    public const string OUTPUT_NAME = "out";
        
    public BinarySustain(string id): base(id, new List<Pin> { new(INPUT_NAME, ValueType.Boolean) }, new List<Pin> { new(OUTPUT_NAME, ValueType.Boolean) })
    {
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
        var input = GetInputValue<Boolean>(INPUT_NAME).value;
        var output = input;
        
        if (input == sustainValue)
        {
            _valueSet ??= context.home.GetTime();
            _valueUnset = null;
        }
        else
        {
            if (_valueSet != null)
            {
                _valueUnset ??= context.home.GetTime();
                    
                var secondsSinceSet = (context.home.GetTime() - _valueSet!).Value.TotalSeconds;
                if (secondsSinceSet < sustainActivateSecs)
                {
                    //  Didn't reach activate time

                    _valueSet = null;
                }
                else
                {
                    //  Sustaining - did we time out yet?
                    
                    var secondsSinceUnset = (context.home.GetTime() - _valueUnset!).Value.TotalSeconds;
                    if (secondsSinceUnset >= sustainDeactivateSecs)
                    {
                        //  Timeout
                        
                        _valueSet = null;
                    }
                    else
                    {
                        //  Sustaining
                        
                        output = sustainValue;
                    }
                }
            }
        }
        
        SetOutputValue(OUTPUT_NAME, new Boolean(output), context);
    }
}