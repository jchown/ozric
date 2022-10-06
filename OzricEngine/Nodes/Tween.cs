using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.Tween)]
public class Tween: Node
{
    public const string INPUT_NAME = "input";
    public const string OUTPUT_NAME = "output";
    public const float UPDATE_INTERVAL_SECS = 2;
        
    public override NodeType nodeType => NodeType.Tween;

    public ValueType valueType { get; set; }
    public float speed { get; set; } = 0.5f;

    [JsonIgnore]
    private DateTime _lastTimeUpdated;

    public Tween(string id, ValueType valueType) : base(id, new() { new(INPUT_NAME, valueType) }, new() { new(OUTPUT_NAME, valueType) })
    {
        this.valueType = valueType;
    }

    public override Task OnInit(Context context)
    {
        var value = GetInput(INPUT_NAME).value;
        if (value != null)
            SetOutputValue(OUTPUT_NAME, value);
        _lastTimeUpdated = context.home.GetTime(); 
        return Task.CompletedTask;
    }

    public override Task OnUpdate(Context context)
    {
        var input = GetInput(INPUT_NAME).value;
        if (input == null)
            return Task.CompletedTask;

        var output = GetOutput(OUTPUT_NAME).value;
        var now = context.home.GetTime();

        if (output == null)
        {
            SetOutputValue(OUTPUT_NAME, input);
            _lastTimeUpdated = now; 
            return Task.CompletedTask;
        }

        if (input.Equals(output))
        {
            _lastTimeUpdated = now; 
            return Task.CompletedTask;
        }

        float dt = (float) (now - _lastTimeUpdated).TotalSeconds;
        if (dt < UPDATE_INTERVAL_SECS)
        {
            return Task.CompletedTask;
        }

        //  https://www.gamedeveloper.com/programming/improved-lerp-smoothing-

        float timeIndependentRate = -(1 / UPDATE_INTERVAL_SECS) * MathF.Log(1 - speed);
        float lerpRate = MathF.Exp(-timeIndependentRate * dt);
        
        Value tweened;
        switch (valueType)
        {
            case ValueType.Scalar:
                var inScalar = (Scalar) input; 
                var outScalar = (Scalar) output;
                tweened = new Scalar(Lerp(outScalar.value, inScalar.value, lerpRate));
                break;
            
            case ValueType.Color:
                var inColor = (ColorValue) input; 
                var outColor = (ColorValue) output; 
                
                //  Special cases:

                //  1. If the input is "off" we should use the output's color mode
                //  so that we retain the correct color space for the chrominance. 

                if (inColor.brightness == 0 && inColor.ColorMode != outColor.ColorMode)
                {
                    inColor = outColor.WithBrightness(0);
                }

                //  2. If the output is "off" we should use the input's color mode
                //  so that we retain the correct color space for the chrominance. 

                if (outColor.brightness == 0 && inColor.ColorMode != outColor.ColorMode)
                {
                    outColor = inColor.WithBrightness(0);
                }

                var brightness = Lerp(outColor.brightness, inColor.brightness, lerpRate);
                switch (inColor.ColorMode)
                {
                    case ColorMode.HS:
                        var inHS = input as ColorHS; 
                        var outHS = output as ColorHS ?? outColor.ToHS();
                        var h = Lerp(outHS.h, inHS.h, lerpRate);
                        var s = Lerp(outHS.s, inHS.s, lerpRate);
                        tweened = new ColorHS(h, s, brightness);
                        break;
                    
                    case ColorMode.Temp:
                        var inT = input as ColorTemp; 
                        var outT = output as ColorTemp ?? outColor.ToTemp();
                        var temp = Lerp(outT.temp, inT.temp, lerpRate);
                        tweened = new ColorTemp((int) temp, brightness);
                        break;
                    
                    case ColorMode.RGB:
                        var inRGB = input as ColorRGB; 
                        var outRGB = output as ColorRGB ?? outColor.ToRGB();
                        var r = Lerp(outRGB.r, inRGB.r, lerpRate);
                        var g = Lerp(outRGB.g, inRGB.g, lerpRate);
                        var b = Lerp(outRGB.b, inRGB.b, lerpRate);
                        tweened = new ColorRGB(r, g, b, brightness);
                        break;
                    
                    case ColorMode.XY:
                        var inXY = input as ColorXY; 
                        var outXY = output as ColorXY ?? outColor.ToXY();
                        var x = Lerp(outXY.x, inXY.x, lerpRate);
                        var y = Lerp(outXY.y, inXY.y, lerpRate);
                        tweened = new ColorXY(x, y, brightness);
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;

            case ValueType.Mode:
            case ValueType.Boolean:
                throw new Exception($"Can't tween {valueType} values");

            default:
                throw new ArgumentOutOfRangeException();
        }
        
        Log(LogLevel.Info, $"Tween {output} -> {input}, {dt:F1}s, lerp {lerpRate * 100:F2}% = {tweened}");

        _lastTimeUpdated = now; 
        SetOutputValue(OUTPUT_NAME, tweened);
        return Task.CompletedTask;
    }

    private float Lerp(float current, float target, float rate)
    {
        return (current * (1 - rate)) + (target * rate);
    }
}