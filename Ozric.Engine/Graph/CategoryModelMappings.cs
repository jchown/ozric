using System;
using System.Collections.Generic;
using Ozric.Engine.Graph.Entities;
using Ozric.Engine.Nodes;

namespace Ozric.Engine.Graph;

public static class CategoryModelMappings
{
    private static readonly Dictionary<Category, Type> ModelMappings = new()
    {
        { Category.Light, typeof(GraphLight) },
        { Category.Switch, typeof(GraphSwitch) },
        { Category.Sensor, typeof(GraphBinarySensor) },
        { Category.ModeSensor, typeof(GraphModeSensor) },
        { Category.MediaPlayer, typeof(GraphMediaPlayer) },
        { Category.Person, typeof(GraphPerson) }
    };
    
    public static bool Exists(Category category)
    {
        return ModelMappings.ContainsKey(category);
    }

    public static Type Get(Category category)
    {
        return ModelMappings[category];
    }
    
    public static Category FromEntityID(string entityId)
    {
        var typeName = entityId.Contains('.') ? entityId.Substring(0, entityId.IndexOf('.')) :entityId;

        return typeName switch
        {
            "light" => Category.Light,
            "switch" => Category.Switch,
            "sensor" => Category.ModeSensor,
            "binary_sensor" => Category.Sensor,
            "media_player" => Category.MediaPlayer,
            "person" => Category.Person,
            _ => Category.Unknown
        };
    }   
}