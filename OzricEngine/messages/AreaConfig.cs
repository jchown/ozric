namespace OzricEngine.messages;

public class AreaConfig
{
    private string[] aliases;
    private string area_id;
    string? icon;
    public string[] labels;
    public string name;
    public string? picture;

    private string? floor_id;
    private string? humidity_entity_id;
    public string? temperature_entity_id;
        
    public double created_at;
    public double modified_at;
}