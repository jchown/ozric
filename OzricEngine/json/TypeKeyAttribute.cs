using System;

namespace OzricEngine
{
    /// <summary>
    /// Attribute that must be placed on derived types 
    /// </summary>
    public class TypeKeyAttribute : Attribute
    {
        public readonly string value;

        public TypeKeyAttribute(string value)
        {
            this.value = value;
        }
    }
}