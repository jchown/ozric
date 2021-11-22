namespace OzricEngine.logic
{
    public abstract class Node
    {
        public readonly string id;
        public string description { get; protected set; }

        protected Node(string id)
        {
            this.id = id;
        }
        
        public abstract void OnInit(Home home);
        
        public abstract void OnUpdate(Home home);
    }
}