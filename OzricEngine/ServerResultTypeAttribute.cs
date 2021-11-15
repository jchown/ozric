using System;

namespace OzricEngine
{
    public class ServerResultTypeAttribute : Attribute
    {
        public readonly string value;

        public ServerResultTypeAttribute(string value)
        {
            this.value = value;
        }
    }
}