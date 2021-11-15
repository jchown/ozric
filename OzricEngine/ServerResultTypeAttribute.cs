using System;

namespace OzricEngine
{
    public class ServerResultTypeAttribute : Attribute
    {
        public readonly ServerResult.Type value;

        public ServerResultTypeAttribute(ServerResult.Type value)
        {
            this.value = value;
        }
    }
}