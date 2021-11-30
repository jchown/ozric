namespace OzricEngine.logic
{
    public class OnOff
    {
        public bool value { get; set;  }

        public override string ToString()
        {
            return value ? "On" : "Off";
        }
    }
}