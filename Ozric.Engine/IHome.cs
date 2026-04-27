using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ozric.Engine.Model;

namespace Ozric.Engine;

/// <summary>
/// The current state of the home.
/// </summary>

public interface IHome
{
    /// <summary>
    /// Area ID for logic nodes that are not in any specific area.
    /// </summary>
    public const string GlobalAreaId = "<home>";
    
    /// <summary>
    /// Area ID for color nodes that are never in any specific area.
    /// </summary>
    public const string PaletteId = "<palette>";
        
    DateTime GetTime();

    void SetUpdatedTime(string entityId);

    bool OnEventStateChanged(EventStateChanged stateChanged);

    List<EntityState> GetEntityStates();

    List<AreaConfig> GetAreas();
    
    List<EntityState> GetEntityStates(List<string> entityIDs);

    EntityState? GetEntityState(string entityId);
    
    bool CanUpdateEntity(EntityState entityState);

    List<DeviceConfig> GetDevicesInArea(string areaId);
    
    List<EntityConfig> GetEntitiesInArea(string areaId);
    
    ReadOnlyCollection<EntityConfig> GetEntityConfigs();
    
    ReadOnlyCollection<DeviceConfig> GetDeviceConfigs();
}