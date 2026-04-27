using Microsoft.AspNetCore.Components;
using Ozric.Engine.Utils;
using LogLevel = Ozric.Engine.Utils.LogLevel;

namespace Ozric.Dashboard;

public class DashboardComponent : ComponentBase
{
    private Logger _logger;
    
    protected Logger Logger => _logger ??= new Logger(GetType().Name);

    protected void Log(LogLevel level, string message) => Logger.Log(level, message);

    protected void Log<T0>(LogLevel level, string message, T0 arg0) => Logger.Log(level, message, arg0);

    protected void Log<T0, T1>(LogLevel level, string message, T0 arg0, T1 arg1) => Logger.Log(level, message, arg0, arg1);

    protected void Log<T0, T1, T2>(LogLevel level, string message, T0 arg0, T1 arg1, T2 arg2) => Logger.Log(level, message, arg0, arg1, arg2);

    protected void Log<T0, T1, T2, T3>(LogLevel level, string message, T0 arg0, T1 arg1, T2 arg2, T3 arg3) => Logger.Log(level, message, arg0, arg1, arg2, arg3);
}
