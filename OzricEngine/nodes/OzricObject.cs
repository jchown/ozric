using System;
using System.Collections;
using System.Globalization;
using System.Text.Json;

namespace OzricEngine.logic
{
    public abstract class OzricObject
    {
        public abstract string Name { get; }
        
        protected LogLevel minLogLevel = LogLevel.Debug;
        
        protected void Log(LogLevel level, string message)
        {
            if (level >= minLogLevel)
                _Log(message);
        }
        
        protected void Log<T0>(LogLevel level, string message, T0 arg0)
        {
            if (level >= minLogLevel)
                _Log(message, arg0);
        }
        
        protected void Log<T0,T1>(LogLevel level, string message, T0 arg0, T1 arg1)
        {
            if (level >= minLogLevel)
                _Log(message, arg0, arg1);
        }
        
        protected void Log<T0,T1,T2>(LogLevel level, string message, T0 arg0, T1 arg1, T2 arg2)
        {
            if (level >= minLogLevel)
                _Log(message, arg0, arg1, arg2);
        }
        
        protected void Log<T0,T1,T2,T3>(LogLevel level, string message, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            if (level >= minLogLevel)
                _Log(message, arg0, arg1, arg2, arg3);
        }

        private void _Log(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} | {Name}: {message}");
        }

        private void _Log(string format, params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is ICollection)
                    args[i] = JsonSerializer.Serialize(args[i]);
            }
            
            _Log(string.Format(format, args));
        }
    }
}