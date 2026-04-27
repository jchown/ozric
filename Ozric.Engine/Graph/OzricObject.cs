using System.Text.Json.Serialization;
using Ozric.Engine.Utils;

namespace Ozric.Engine.Graph;

public abstract class OzricObject
{
    [JsonIgnore]
    public abstract string Name { get; }

    private Logger? _logger;
    private Logger Logger => _logger ??= new Logger(Name);

    protected void Log(LogLevel level, string message) => Logger.Log(level, message);

    protected void Log<T0>(LogLevel level, string message, T0 arg0) => Logger.Log(level, message, arg0);

    protected void Log<T0, T1>(LogLevel level, string message, T0 arg0, T1 arg1) => Logger.Log(level, message, arg0, arg1);

    protected void Log<T0, T1, T2>(LogLevel level, string message, T0 arg0, T1 arg1, T2 arg2) => Logger.Log(level, message, arg0, arg1, arg2);

    protected void Log<T0, T1, T2, T3>(LogLevel level, string message, T0 arg0, T1 arg1, T2 arg2, T3 arg3) => Logger.Log(level, message, arg0, arg1, arg2, arg3);

    public override string ToString()
    {
        return Json.Serialize(this, GetType());
    }
}
