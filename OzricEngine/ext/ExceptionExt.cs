using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OzricEngine.ext
{
    public static class ExceptionExt
    {
        class RethrownException: Exception
        {
            private readonly string stackTrace;

            public RethrownException(string message, string stackTrace): base(message)
            {
                this.stackTrace = stackTrace;
            }

            public override string StackTrace => stackTrace;
        }
        
        public static Exception Rethrown(this Exception e, string contextMessage)
        {
            return new RethrownException($"{e.Message}\n{contextMessage}", e.StackTrace);
        }
    }
}