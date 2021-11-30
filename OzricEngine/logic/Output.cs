namespace OzricEngine.logic
{
    public class Output
    {
        public string name { get; }
        public object value { get; protected set;  }
        
        public Output(string name, object value)
        {
            this.name = name;
            this.value = value;
        }
    }
}