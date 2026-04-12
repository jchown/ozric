using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Ozric.Engine.Graph;
using OzricEngine.Values;

namespace Ozric.Dashboard.Model;

public class OzricLinkModel : LinkModel
{
    public string StripeColor { get; set; } = "#AAAAAA";

    public OzricLinkModel(PortModel sourcePort, PortModel targetPort) : base(sourcePort, targetPort)
    {
    }

    public void UpdateColors(Pin pin)
    {
        var (baseColor, stripeColor) = ComputeColors(pin.value);
        Color = baseColor;
        StripeColor = stripeColor;
    }

    public static (string baseColor, string stripeColor) ComputeColors(Value? value)
    {
        switch (value)
        {
            case Binary binary:
            {
                var baseColor = binary.value ? "#4CAF50" : "#666666";
                var stripe = binary.value ? "#CDDC39" : "#999999";
                return (baseColor, stripe);
            }

            case ColorValue color:
            {
                if (color.brightness == 0)
                    return ("#333333", "#555555");

                var hex = "#" + color.ToHexString();
                var stripe = ShiftedColor(hex);
                return (hex, stripe);
            }

            case Number number:
            {
                // Interpolate hue from blue (240) at 0.0 to orange (30) at 1.0
                var t = Math.Clamp(number.value, 0f, 1f);
                var hue = 240.0 - t * 210.0; // 240 -> 30
                var baseColor = HslToHex(hue, 0.7, 0.5);
                var stripe = HslToHex((hue + 180.0) % 360.0, 0.7, 0.5);
                return (baseColor, stripe);
            }

            case Mode mode:
            {
                var hue = HueFromHash(mode.value);
                var baseColor = HslToHex(hue, 0.65, 0.5);
                var stripe = HslToHex((hue + 180.0) % 360.0, 0.65, 0.5);
                return (baseColor, stripe);
            }

            default:
                return ("#888888", "#AAAAAA");
        }
    }

    public static string ComplementaryColor(string hex)
    {
        HexToHsl(hex, out var h, out var s, out var l);

        // For greys (very low saturation), just shift lightness
        if (s < 0.1)
        {
            var newL = l > 0.5 ? l - 0.3 : l + 0.3;
            return HslToHex(h, s, Math.Clamp(newL, 0, 1));
        }

        return HslToHex((h + 180.0) % 360.0, s, l);
    }

    /// <summary>
    /// For Color values: keep the same hue, shift saturation/lightness to create contrast.
    /// </summary>
    public static string ShiftedColor(string hex)
    {
        HexToHsl(hex, out var h, out var s, out var l);

        // Shift lightness: if dark, go lighter; if light, go darker
        var newL = l > 0.5 ? l - 0.25 : l + 0.25;
        // Shift saturation: desaturate if saturated, boost if not
        var newS = s > 0.5 ? s - 0.3 : s + 0.3;

        return HslToHex(h, Math.Clamp(newS, 0, 1), Math.Clamp(newL, 0.1, 0.9));
    }

    public static double HueFromHash(string name)
    {
        // Use a simple hash to get a deterministic hue
        uint hash = 0;
        foreach (var c in name)
        {
            hash = hash * 31 + c;
        }

        return (hash % 360);
    }

    private static string HslToHex(double h, double s, double l)
    {
        // HSL to RGB conversion
        var c = (1.0 - Math.Abs(2.0 * l - 1.0)) * s;
        var x = c * (1.0 - Math.Abs((h / 60.0) % 2.0 - 1.0));
        var m = l - c / 2.0;

        double r, g, b;

        if (h < 60) { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }

        var ri = (int)((r + m) * 255);
        var gi = (int)((g + m) * 255);
        var bi = (int)((b + m) * 255);

        return $"#{ri:X2}{gi:X2}{bi:X2}";
    }

    private static void HexToHsl(string hex, out double h, out double s, out double l)
    {
        hex = hex.TrimStart('#');
        if (hex.Length < 6)
        {
            h = 0; s = 0; l = 0.5;
            return;
        }

        var r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255.0;
        var g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255.0;
        var b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        l = (max + min) / 2.0;

        if (delta == 0)
        {
            h = 0;
            s = 0;
        }
        else
        {
            s = l > 0.5 ? delta / (2.0 - max - min) : delta / (max + min);

            if (max == r)
                h = ((g - b) / delta + (g < b ? 6 : 0)) * 60.0;
            else if (max == g)
                h = ((b - r) / delta + 2) * 60.0;
            else
                h = ((r - g) / delta + 4) * 60.0;
        }
    }
}
