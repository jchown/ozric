using OzricEngine.engine;

namespace OzricEngine.Nodes
{
    public class Context
    {
        public Context(Home home, CommandBatcher commands, Pin.Changed? pinChanged, Alert.Changed? alertChanged)
        {
            this.home = home;
            this.commands = commands;
            this.pinChanged += pinChanged;
            this.alertChanged += alertChanged;
        }

        public Home home { get;}
        public CommandBatcher commands { get;}
        public Pin.Changed? pinChanged { get;}
        public Alert.Changed? alertChanged { get;}
    }
}