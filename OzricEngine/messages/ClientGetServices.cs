using System.Collections.Generic;

namespace OzricEngine
{
    [TypeKey("get_services")]
    public class ClientGetServices : ClientCommand
    {
        public ClientGetServices() : base("get_services")
        {
        }
    }
}