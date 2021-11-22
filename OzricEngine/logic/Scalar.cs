namespace OzricEngine.logic
{
    public abstract class Scalar: Node
    {
        public float value { get; protected set;  }

        protected Scalar(string id) : base(id)
        {
        }
    }
}