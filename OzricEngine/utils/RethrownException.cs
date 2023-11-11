using System;

namespace OzricEngine
{
    /// <summary>
    /// Helper to give context to an exception. For example, if you do not handle the exception but have
    /// contextual information to impart.
    ///
    /// try
    /// {
    ///     Load(filename);
    ///     Parse(filename);
    ///     Validate(filename);
    /// }
    /// catch (Exception e)
    /// {
    ///     throw new RethrownException(e, $"while processing {filename}"); 
    /// }
    /// </summary>
    public class RethrownException : Exception
    {
        private readonly Exception _e;

        public RethrownException(Exception e, string context) : base($"{Title(e)}\n... {context}")
        {
            _e = e;
        }

        private static string Title(Exception e)
        {
            var message = e.Message;
            if (e is RethrownException)
                return message;

            var className = e.GetType().ToString();
            return message.Length <= 0 ? className : $"{className}: {message}";
        }

        public override string? Source => _e.Source;

        public override string? StackTrace => _e.StackTrace;
    }
}