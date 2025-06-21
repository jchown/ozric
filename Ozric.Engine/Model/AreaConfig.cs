namespace Ozric.Engine.Model;

public class AreaConfig
{
    public string name;
    public string area_id;
    public string[] aliases;
    public string? icon;
    public string[] labels;
    public string? picture;

    public string? floor_id;
    public string? humidity_entity_id;
    public string? temperature_entity_id;
        
    public double created_at;
    public double modified_at;
}