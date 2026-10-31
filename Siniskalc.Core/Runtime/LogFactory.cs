namespace Siniskalc.Core.Runtime
{
    using System;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Where a class gets its logger: <c>private static readonly ILogger&lt;T&gt; Logger = LogFactory.For&lt;T&gt;();</c>. The application installs
    /// the real <see cref="ILoggerFactory"/> once, at start-up (<see cref="Install"/>); until then, and in the tests, the loggers write
    /// nowhere. This is the one place the repo allows a static collaborator (see docs/LOGGING.md).
    /// </summary>
    public static class LogFactory
    {
        private static volatile ILoggerFactory current = NullLoggerFactory.Instance;

        /// <summary>
        /// Gets the factory the loggers of this process come from.
        /// </summary>
        internal static ILoggerFactory Current => current;

        /// <summary>
        /// Gets the logger of the class <typeparamref name="T"/>, which logs to whatever factory is installed when it is used,
        /// so it can be kept in a static field that is created before <see cref="Install"/> runs.
        /// </summary>
        /// <typeparam name="T">The class whose name the log lines show.</typeparam>
        /// <returns>The logger of <typeparamref name="T"/>.</returns>
        public static ILogger<T> For<T>() => new ForwardingLogger<T>();

        /// <summary>
        /// Gets the logger of the class <paramref name="type"/>, for a static class (which cannot be a type argument) or an open generic type.
        /// </summary>
        /// <param name="type">The class whose name the log lines show; for a generic type, its name without the type arguments.</param>
        /// <returns>The logger of <paramref name="type"/>.</returns>
        public static ILogger For(Type type) => new ForwardingLogger(ForwardingLogger.CategoryOf(type));

        /// <summary>
        /// Makes <paramref name="factory"/> the factory every logger of this process writes through.
        /// </summary>
        /// <param name="factory">The factory to write through.</param>
        public static void Install(ILoggerFactory factory) => current = factory;

        /// <summary>
        /// Goes back to loggers that write nowhere.
        /// </summary>
        public static void Reset() => current = NullLoggerFactory.Instance;
    }
}
