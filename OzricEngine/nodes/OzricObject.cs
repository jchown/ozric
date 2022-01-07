using System;

namespace OzricEngine.logic
{
    public abstract class OzricObject
    {
        public abstract string Name { get; }
        
        private LogLevel minLevel = LogLevel.Info;
        
        protected void Log(LogLevel level, string message)
        {
            if (level >= minLevel)
                _Log(message);
        }
        
        protected void Log<T0>(LogLevel level, string message, T0 arg0)
        {
            if (level >= minLevel)
                _Log(message, arg0);
        }
        
        protected void Log<T0,T1>(LogLevel level, string message, T0 arg0, T1 arg1)
        {
            if (level >= minLevel)
                _Log(message, arg0, arg1);
        }
        
        protected void Log<T0,T1,T2>(LogLevel level, string message, T0 arg0, T1 arg1, T2 arg2)
        {
            if (level >= minLevel)
                _Log(message, arg0, arg1, arg2);
        }
        
        protected void Log<T0,T1,T2,T3>(LogLevel level, string message, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            if (level >= minLevel)
                _Log(message, arg0, arg1, arg2, arg3);
        }

        private void _Log(string message)
        {
            Console.WriteLine($"[{Name}] {message}");
        }

        private void _Log(string format, params object[] args)
        {
            _Log(string.Format(format, args));
        }
    }
}