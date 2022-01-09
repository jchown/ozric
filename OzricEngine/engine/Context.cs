namespace OzricEngine.logic
{
    public class Context
    {
        public Context(Engine engine, Engine.ICommandSender commandSender)
        {
            this.engine = engine;
            this.commandSender = commandSender;
        }

        public Engine engine { get;}
        public Engine.ICommandSender commandSender { get;}
    }
}