namespace Siniskalc.Core.Runtime
{
    using System;

    /// <summary>
    /// Logging could not be started with what the user asked for (a log folder that cannot be created or written, a Serilog
    /// configuration that cannot be read or built). The message is written for the user and names the folder or file.
    /// </summary>
    public sealed class LoggingException : Exception
    {
        /// <param name="message">What went wrong, for the user.</param>
        /// <param name="innerException">The failure behind it.</param>
        /// <param name="path">The configuration file that is damaged, or <c>null</c> when the failure is not about a file the user can reset.</param>
        public LoggingException(string message, Exception innerException, string? path = null)
            : base(message, innerException)
        {
            this.Path = path;
        }

        /// <summary>
        /// Gets the configuration file the failure is about, or <c>null</c>.
        /// </summary>
        public string? Path { get; }
    }
}
