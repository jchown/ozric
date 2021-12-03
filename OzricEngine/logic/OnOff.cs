namespace OzricEngine.logic
{
    public class OnOff
    {
        public bool value { get; set;  }

        public OnOff(bool value = false)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value ? "On" : "Off";
        }
    }
}