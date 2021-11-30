namespace OzricEngine.logic
{
    public class Input
    {
        public string name { get; }
        public object value { get; protected set;  }
        
        public Input(string name, object value)
        {
            this.name = name;
            this.value = value;
        }
    }
}