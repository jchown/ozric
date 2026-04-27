namespace Ozric.Engine
{
    [TypeKey("get_services")]
    public class ClientGetServices : ClientCommand
    {
        public ClientGetServices() : base("get_services")
        {
        }
    }
}