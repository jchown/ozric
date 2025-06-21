using System.Collections.Generic;

namespace OzricEngine
{
    [TypeKey("get_config")]
    public class ClientGetConfig : ClientCommand
    {
        public ClientGetConfig() : base("get_config")
        {
        }
    }
}