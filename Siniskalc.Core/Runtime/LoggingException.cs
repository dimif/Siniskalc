namespace Siniskalc.Core.Runtime
{
    using System;

    /// <summary>
    /// Logging could not be started with what the user asked for (a log folder that cannot be created or written, a level that
    /// means nothing). The message is written for the user and names the folder or setting.
    /// </summary>
    public sealed class LoggingException : Exception
    {
        public LoggingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
