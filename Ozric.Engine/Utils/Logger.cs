using System;
using System.Collections;
using System.Globalization;
using Humanizer;

namespace Ozric.Engine.Utils;

public class Logger
{
    public static Action<string> Output = line => Console.WriteLine(line);

    private readonly Func<string> _name;

    public LogLevel MinLevel { get; set; } = LogLevel.Info;

    public Logger(string name) : this(() => name)
    {
    }

    public Logger(Func<string> name)
    {
        _name = name;
    }

    public void Log(LogLevel level, string message)
    {
        if (level >= MinLevel)
            Write(level, message);
    }

    public void Log<T0>(LogLevel level, string message, T0 arg0)
    {
        if (level >= MinLevel)
            Write(level, message, arg0);
    }

    public void Log<T0, T1>(LogLevel level, string message, T0 arg0, T1 arg1)
    {
        if (level >= MinLevel)
            Write(level, message, arg0, arg1);
    }

    public void Log<T0, T1, T2>(LogLevel level, string message, T0 arg0, T1 arg1, T2 arg2)
    {
        if (level >= MinLevel)
            Write(level, message, arg0, arg1, arg2);
    }

    public void Log<T0, T1, T2, T3>(LogLevel level, string message, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
    {
        if (level >= MinLevel)
            Write(level, message, arg0, arg1, arg2, arg3);
    }

    private static readonly string[] colors =
    {
        "[90m",            // Trace (dark grey)
        "[37m",            // Debug (light grey)
        "[97m",            // Info (black)
        "[93m",            // Warning (orange)
        "[91m",            // Error (red)
        "[93m[101m", // Fatal
    };

    private void Write(LogLevel level, string message)
    {
        var name = _name();
        Output($"{colors[(int)level]}{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} | {name.Truncate(32, name, TruncateFrom.Left).PadRight(32)} | {message}[0m");
    }

    private void Write(LogLevel level, string format, params object?[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg is ICollection)
                args[i] = Json.Serialize(arg);
        }

        Write(level, string.Format(format, args));
    }
}
