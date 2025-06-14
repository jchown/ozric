using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OzricEngine;

/// <summary>
/// The current state of the home.
/// </summary>

public interface IHome
{
    DateTime GetTime();

    void SetUpdatedTime(string entityId);

    bool OnEventStateChanged(EventStateChanged stateChanged);

    List<EntityState> GetEntityStates();
    
    List<EntityState> GetEntityStates(List<string> entityIDs);

    EntityState? GetEntityState(string entityId);
    
    bool CanUpdateEntity(EntityState entityState);
}