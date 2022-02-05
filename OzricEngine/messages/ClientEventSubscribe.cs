namespace OzricEngine
{
    [TypeKey("subscribe_events")]
    public class ClientEventSubscribe: ClientCommand
    {
        public string event_type { get; set; } = null;

        public ClientEventSubscribe() : base("subscribe_events")
        {
        }
    }
}