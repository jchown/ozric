using System;
using System.Collections;
using System.Globalization;
using System.Text.Json.Serialization;
using Humanizer;

namespace OzricEngine.Nodes;

public abstract class OzricObject
{
    [JsonIgnore]
    public abstract string Name { get; }
    
    public static Action<string> LogOutput = (line) => Console.WriteLine(line);
        
    protected LogLevel minLogLevel = LogLevel.Info;
        
    protected void Log(LogLevel level, string message)
    {
        if (level >= minLogLevel)
            _Log(level, message);
    }
        
    protected void Log<T0>(LogLevel level, string message, T0 arg0)
    {
        if (level >= minLogLevel)
            _Log(level, message, arg0);
    }
        
    protected void Log<T0,T1>(LogLevel level, string message, T0 arg0, T1 arg1)
    {
        if (level >= minLogLevel)
            _Log(level, message, arg0, arg1);
    }
        
    protected void Log<T0,T1,T2>(LogLevel level, string message, T0 arg0, T1 arg1, T2 arg2)
    {
        if (level >= minLogLevel)
            _Log(level, message, arg0, arg1, arg2);
    }
        
    protected void Log<T0,T1,T2,T3>(LogLevel level, string message, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
    {
        if (level >= minLogLevel)
            _Log(level, message, arg0, arg1, arg2, arg3);
    }

    private static readonly string[] colours =
    {
        "\u001b[90m",   // Trace
        "\u001b[37m",   // Debug
        "\u001b[97m",   // Info
        "\u001b[93m",   // Warning
        "\u001b[91m",   // Error
        "\u001b[93m\u001b[101m",   // Fatal
    };

    private void _Log(LogLevel level, string message)
    {
        LogOutput($"{colours[(int)level]}{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} | {Name.Truncate(32, Name, TruncateFrom.Left).PadRight(32)} | {message}\u001b[0m");
    }

    private void _Log(LogLevel level, string format, params object?[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg is ICollection)
                args[i] = Json.Serialize(arg);
        }
            
        _Log(level, string.Format(format, args));
    }

    public override string ToString()
    {
        return Json.Serialize(this, GetType());
    }
}