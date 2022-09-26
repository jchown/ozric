using System;
using System.Collections.Generic;
using OzricEngine.Nodes;

namespace OzricEngine.Nodes;

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
}