using System;

namespace OzricEngine.engine;

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