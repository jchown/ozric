namespace Ozric.Engine
{
    [TypeKey("ping")]
    public class ClientPing : ClientCommand
    {
        public ClientPing() : base("ping") {}
    }
}