using System;
using System.Collections.Generic;
using Ozric.Engine.Graph;
using OzricEngine;
using OzricEngine.Nodes;

namespace Ozric.Engine.Graph.Entities;

/// <summary>
/// A node that maps to a HomeAssistant entity.
/// </summary>
public abstract class EntityGraphNode: GraphNode
{
    public string entityID { get; }

//    [JsonIgnore]
//    private DateTime? lastTimeRecordedEntity;

    protected EntityGraphNode(string id, string entityID, List<Pin>? inputs, List<Pin>? outputs): base(id, inputs, outputs)
    {
        this.entityID = entityID;
    }

    protected float GetSecondsSinceLastUpdated(Home home)
    {
        var state = home.GetEntityState(entityID);
        if (state == null)
            return float.MaxValue;

        return (float) (home.GetTime() - state.last_updated).TotalSeconds;
    }

    public override bool Equals(object? obj)
    {
        if (!(obj is EntityGraphNode entity))
            return false;
        
        return base.Equals(obj) && entityID == entity.entityID;
    }
        
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), entityID);
    }

}