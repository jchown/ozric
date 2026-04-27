using System;
using Ozric.Engine.Utils;

namespace Ozric.Engine.Live;

public class Alert
{
    public LogLevel Level { get; }
    public string Message { get; }
    public DateTime Started, Latest;
    
    public delegate void Changed(string nodeID);

    public Alert(LogLevel level, string message)
    {
        Level = level;
        Message = message;
        Started = DateTime.Now;
        Latest = Started;
    }
}