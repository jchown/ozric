using System;
using System.Collections.Generic;
using OzricEngine.logic;

namespace OzricEngine.nodes;

public static class CategoryModelMappings
{
    private static readonly Dictionary<Category, Type> ModelMappings = new()
    {
        { Category.Light, typeof(Light) },
        { Category.Switch, typeof(Switch) },
        { Category.Sensor, typeof(Sensor) },
        { Category.MediaPlayer, typeof(MediaPlayer) }
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