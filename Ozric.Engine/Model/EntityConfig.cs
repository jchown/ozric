using Ozric.Engine.Graph;
using Ozric.Engine.Messages;
using Ozric.Engine.Nodes;
using OzricEngine;
using OzricEngine.Nodes;

namespace Ozric.Engine.Model;

public class EntityConfig
{
    public string? area_id;
    public Attributes categories;
    public string? config_entry_id;
    public string? config_subentry_id;
    public double created_at;
        
    public string? device_id;
    public string? disabled_by;
    public string? entity_category;
    public string entity_id;
    public bool has_entity_name;
    public string? hidden_by;
    public string? icon;
    public string id;
    public string[] labels;
        
    public double modified_at;
    public string? name;
    public Attributes options;
        
    public string original_name;
    public string platform;
    public string? translation_key;
    public string unique_id;

    public Category GetCategory()
    {
        return CategoryModelMappings.FromEntityID(entity_id);
    }
}