using System;

namespace Ozric.Engine.Extensions;

public static class ExceptionExt
{
    class RethrownException: Exception
    {
        public RethrownException(string message, string? stackTrace): base(message)
        {
            StackTrace = stackTrace;
        }

        public override string? StackTrace { get; }
    }
        
    public static Exception Rethrown(this Exception e, string contextMessage)
    {
        return new RethrownException($"{e.Message}\n{contextMessage}", e.StackTrace);
    }
}