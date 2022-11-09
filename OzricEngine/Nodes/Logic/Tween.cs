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
            SetOutputValue(OUTPUT_NAME, value, context);
        _lastTimeUpdated = context.home.GetTime(); 
        return Task.CompletedTask;
    }

    public override Task OnUpdate(Context context)
    {
        Update(context);
        
        return Task.CompletedTask;
    }

    private void Update(Context context)
    {
        var input = GetInput(INPUT_NAME).value;
        if (input == null)
            return;

        var output = GetOutput(OUTPUT_NAME).value;
        var now = context.home.GetTime();

        if (output == null)
        {
            SetOutputValue(OUTPUT_NAME, input, context);
            _lastTimeUpdated = now;
            return;
        }

        if (input.Equals(output))
        {
            _lastTimeUpdated = now;
            return;
        }

        float dt = (float)(now - _lastTimeUpdated).TotalSeconds;
        if (dt < UPDATE_INTERVAL_SECS)
            return;

        var lerpRate = GetLerpRate(dt, speed);

        Value result;
        switch (valueType)
        {
            case ValueType.Number:
                var inNum = (Number)input;
                var outNum = (Number)output;
                result = new Number(Lerp(outNum.value, inNum.value, lerpRate));
                break;

            case ValueType.Color:
                var inColor = (ColorValue)input;
                var outColor = (ColorValue)output;

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
                        var inHS = (ColorHS)input;
                        var outHS = output as ColorHS ?? outColor.ToHS();
                        var h = Lerp(outHS.h, inHS.h, lerpRate);
                        var s = Lerp(outHS.s, inHS.s, lerpRate);
                        result = new ColorHS(h, s, brightness);
                        break;

                    case ColorMode.Temp:
                        var inT = (ColorTemp)input;
                        var outT = output as ColorTemp ?? outColor.ToTemp();
                        var temp = Lerp(outT.temp, inT.temp, lerpRate);
                        result = new ColorTemp((int)temp, brightness);
                        break;

                    case ColorMode.RGB:
                        var inRGB = (ColorRGB)input;
                        var outRGB = output as ColorRGB ?? outColor.ToRGB();
                        var r = Lerp(outRGB.r, inRGB.r, lerpRate);
                        var g = Lerp(outRGB.g, inRGB.g, lerpRate);
                        var b = Lerp(outRGB.b, inRGB.b, lerpRate);
                        result = new ColorRGB(r, g, b, brightness);
                        break;

                    case ColorMode.XY:
                        var inXY = (ColorXY)input;
                        var outXY = output as ColorXY ?? outColor.ToXY();
                        var x = Lerp(outXY.x, inXY.x, lerpRate);
                        var y = Lerp(outXY.y, inXY.y, lerpRate);
                        result = new ColorXY(x, y, brightness);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;

            case ValueType.Mode:
            case ValueType.Binary:
                throw new Exception($"Can't tween {valueType} values");

            default:
                throw new ArgumentOutOfRangeException();
        }

        Log(LogLevel.Info, $"Tween {output} -> {input}, {dt:F1}s, lerp {lerpRate * 100:F2}% = {result}");

        _lastTimeUpdated = now;
        SetOutputValue(OUTPUT_NAME, result, context);
    }

    public static float GetLerpRate(float dt, float speed)
    {
        //  https://www.gamedeveloper.com/programming/improved-lerp-smoothing-

        float timeIndependentRate = -(1 / UPDATE_INTERVAL_SECS) * MathF.Log(1 - speed);
        return MathF.Exp(-timeIndependentRate * dt);
    }

    public static float Lerp(float current, float target, float rate)
    {
        return (current * (1 - rate)) + (target * rate);
    }
}