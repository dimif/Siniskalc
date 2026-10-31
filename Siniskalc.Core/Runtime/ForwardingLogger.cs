namespace Siniskalc.Core.Runtime
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A logger that hands every call to the logger the installed <see cref="LogFactory"/> makes for its category. It looks the factory up
    /// when it is used and again whenever another factory is installed, so a logger kept in a static field never stays bound to the
    /// factory that was there when the field was created.
    /// </summary>
    internal class ForwardingLogger : ILogger
    {
        private readonly string category;
        private volatile Resolved? resolved;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardingLogger"/> class.
        /// </summary>
        /// <param name="category">The category the lines are logged under, usually the full name of a class.</param>
        public ForwardingLogger(string category)
        {
            this.category = category;
        }

        /// <summary>
        /// Returns the name logs show for <paramref name="type"/>: its full name, without the arity of a generic type.
        /// </summary>
        /// <param name="type">The class the lines are about.</param>
        /// <returns>The full name of <paramref name="type"/>, such as <c>Siniskalc.Core.Data.ChunkCollection</c> for <c>ChunkCollection&lt;T&gt;</c>.</returns>
        public static string CategoryOf(Type type)
        {
            var name = type.FullName ?? type.Name;
            var generic = name.IndexOf('`', StringComparison.Ordinal);
            return generic < 0 ? name : name[..generic];
        }

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => this.Target().BeginScope(state);

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => this.Target().IsEnabled(logLevel);

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => this.Target().Log(logLevel, eventId, state, exception, formatter);

        private ILogger Target()
        {
            var factory = LogFactory.Current;
            var known = this.resolved;
            if (known is null || !ReferenceEquals(known.Factory, factory))
            {
                known = new Resolved(factory, factory.CreateLogger(this.category));
                this.resolved = known;
            }

            return known.Logger;
        }

        private sealed record Resolved(ILoggerFactory Factory, ILogger Logger);
    }
}
