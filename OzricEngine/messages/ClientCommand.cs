namespace OzricEngine
{
    public abstract class ClientCommand
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

        public override string ToString()
        {
            return Json.Serialize(this);
        }
    }
}