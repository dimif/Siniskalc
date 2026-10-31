namespace Siniskalc.Core.Runtime
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Names what a log line is about, so the line does not have to repeat it (see docs/LOGGING.md).
    /// </summary>
    public static class LogScopeExtensions
    {
        /// <summary>
        /// The name of the property the log file shows between the category and the message.
        /// </summary>
        public const string SubjectProperty = "Subject";

        /// <summary>
        /// Begins a scope in which every line is about the <paramref name="kind"/> called <paramref name="name"/>, such as <c>Hero Sandro</c>.
        /// Dispose it when the work on that one record is done; use it with <c>using var</c>.
        /// </summary>
        public static IDisposable? BeginSubject(this ILogger logger, string kind, string name) =>
            logger.BeginScope(new Dictionary<string, object> { [SubjectProperty] = kind + " " + name });
    }
}
