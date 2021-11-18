namespace OzricEngine
{
    public class ClientCommand
    {
        public int id { get; set; } = NextID++;
        public string type { get; set; }

        protected ClientCommand(string type)
        {
            this.type = type;
        }
        
        private static int NextID = 1;
    }
}