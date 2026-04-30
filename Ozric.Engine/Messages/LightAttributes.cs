namespace Ozric.Engine;

/// <summary>
/// See https://developers.home-assistant.io/docs/core/entity/light/
/// </summary>
public class LightAttributes
{
    public int? brightness;
        
    public string color_mode;
    public string[] supported_color_modes;

    public int? color_temp;
    public int? color_temp_kelvin;
    public float[]? hs_color ;
    public int[]? rgb_color ;
    public int[]? rgbw_color ;
    public int[]? rgbww_color ;
    public float[]? xy_color ;
        
    public string effect ;
    public string[] effect_list ;
    public string friendly_name ;

    public int min_mireds;
    public int max_mireds;
        
    public int supported_features;
}