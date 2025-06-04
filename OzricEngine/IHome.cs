using System;
using System.Collections.Generic;

namespace OzricEngine;

public interface IHome
{
    DateTime GetTime();

    void SetUpdatedTime(string entityId);

    bool OnEventStateChanged(EventStateChanged stateChanged);

    List<EntityState> GetEntityStates();
    
    List<EntityState> GetEntityStates(List<string> getInterestedEntityIDs);

    EntityState GetEntityState(string entityId);
    
    bool CanUpdateEntity(EntityState entityState);
}