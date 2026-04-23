using System;

namespace Ozric.Engine.Extensions;

public static class TypeExt
{
    /// <summary>
    /// If true, the given interface is concretely implemented by the given class.
    /// </summary>
    public static bool Implements(this Type type, Type interfaceType)
    {
        return !type.IsAbstract && interfaceType.IsInterface && interfaceType.IsAssignableFrom(type);
    }
}