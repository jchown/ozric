namespace OzricEngine
{
    public class ClientCommand
    {
        /// <summary>
        /// Unique ID assigned by Comms just before sending
        /// </summary>
        public int id { get; set; }

        public string type { get; set; }

        protected ClientCommand(string type)
        {
            this.type = type;
        }
    }
}