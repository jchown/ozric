using Ozric.Engine.Messages;

namespace Ozric.Engine.Model;

public class DeviceConfig
{
    public string id;
 
    public string name;
    public string manufacturer;
    public string model;
    public string serial_number;
    public string? hw_version;
    public string? sw_version;

    
    public string? name_by_user;
    public string? via_device_id;
    public string area_id;
    public string primary_config_entry;
    public string? entry_type;
    public string? configuration_url;
    public string[] labels;
    public string[] config_entries;
    public Attributes config_entries_subentries;
    public string[][] connections;
    public string[][] identifiers;
    
    public string? disabled_by;
        
    public double created_at;
    public double modified_at;        
}