namespace OzricEngine.logic
{
    public class EngineLifecycleContext
    {
        public EngineLifecycleContext(Engine engine, MergingCommandSender commandSender)
        {
            this.engine = engine;
            this.commandSender = commandSender;
        }

        public Engine engine { get;}
        public Engine.ICommandSender commandSender { get;}
    }
}