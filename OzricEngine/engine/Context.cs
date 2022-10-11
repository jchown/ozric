namespace OzricEngine.Nodes
{
    public class Context
    {
        public Context(Home home, CommandBatcher commands, Pin.Changed? pinChanged)
        {
            this.home = home;
            this.commands = commands;
            this.pinChanged += pinChanged;
        }

        public Home home { get;}
        public CommandBatcher commands { get;}
        public Pin.Changed? pinChanged { get;}
    }
}