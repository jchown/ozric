using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ozric.Engine.Model;
using OzricEngine.messages;

namespace OzricEngine;

/// <summary>
/// The current state of the home.
/// </summary>

public interface IHome
{
    /// <summary>
    /// Area ID for nodes that are not in any specific area.
    /// </summary>
    public const string GlobalAreaId = "<home>";
        
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