namespace OzricEngine
{
    [TypeKey("ping")]
    public class ClientPing : ClientCommand
    {
        public ClientPing() : base("ping") {}
    }
}