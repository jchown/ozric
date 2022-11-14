using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.NumberCompare)]
public class NumberCompare: Node
{
    public override NodeType nodeType => NodeType.NumberCompare;

    public const string InputName = "input";
    public const string OutputName = "output";
    
    public enum Comparator
    {
        EqualTo,
        EqualToApprox,
        LessThan,
        LessThanOrEqualTo,
        GreaterThan,
        GreaterThanOrEqualTo,
        BetweenInclusive,
        BetweenExclusive
    }

    public Comparator comparator { get; set; }
    public float a { get; set; }
    public float b { get; set; }

    public NumberCompare(string id): base(id, new List<Pin> { new(InputName, ValueType.Number) }, new List<Pin> { new(OutputName, ValueType.Binary) })
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
        if (!HasInputValue(InputName))
            return;
        
        var value = GetInputValue<Number>(InputName).value;
        bool result;

        switch (comparator)
        {
            case Comparator.LessThan:
                result = value < a;
                break;
            
            case Comparator.LessThanOrEqualTo:
                result = value <= a;
                break;
            
            case Comparator.GreaterThan:
                result = value > a;
                break;
            
            case Comparator.GreaterThanOrEqualTo:
                result = value >= a;
                break;
            
            case Comparator.EqualTo:
                result = MathF.Abs(value - a) < b;
                break;
            
            case Comparator.BetweenInclusive:
                result = a <= value && value <= b;
                break;
            
            case Comparator.BetweenExclusive:
                result = a < value && value < b;
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }

        SetOutputValue(OutputName, new Binary(result), context);
    }
}