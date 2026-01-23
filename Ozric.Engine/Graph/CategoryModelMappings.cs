using System;
using System.Collections.Generic;
using Ozric.Engine.Nodes;
using OzricEngine.Nodes;

namespace Ozric.Engine.Graph;

public static class CategoryModelMappings
{
    private static readonly Dictionary<Category, Type> ModelMappings = new()
    {
        { Category.Light, typeof(Light) },
        { Category.Switch, typeof(Switch) },
        { Category.Sensor, typeof(BinarySensor) },
        { Category.ModeSensor, typeof(ModeSensor) },
        { Category.MediaPlayer, typeof(MediaPlayer) },
        { Category.Person, typeof(Person) }
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