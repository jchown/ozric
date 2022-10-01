namespace OzricEngine.Nodes
{
    public class Context
    {
        public Context(Home home, CommandBatcher commands)
        {
            this.home = home;
            this.commands = commands;
        }

        public Home home { get;}
        public CommandBatcher commands { get;}
    }
}