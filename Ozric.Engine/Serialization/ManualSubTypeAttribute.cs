using System;

namespace OzricEngine
{
    /// <summary>
    /// Attribute that must be placed on derived types that aren't directly used in polymorphic JSON serialization
    /// </summary>
    public class ManualSubTypeAttribute : Attribute
    {
    }
}