namespace OzricEngine
{
    public class ClientEventSubscribe
    {
        public int id { get; set; } = NextID++;
        public string type { get; set; } = "subscribe_events";
        public string event_type { get; set; } = null;

        private static int NextID = 1;
    }
}