namespace Siniskalc.Core.Tests
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A logger that keeps what it is given, for the tests whose point is that something is logged.
    /// </summary>
    internal sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => this.Entries.Add((logLevel, formatter(state, exception), exception));
    }
}
