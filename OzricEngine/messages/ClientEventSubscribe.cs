namespace OzricEngine
{
    [TypeKey("subscribe_events")]
    public class ClientEventSubscribe: ClientCommand
    {
        public string? event_type { get; set; }

        public ClientEventSubscribe() : base("subscribe_events")
        {
        }
    }
}