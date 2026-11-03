namespace Siniskalc.Wpf.ViewModels
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// One entry of the list of log levels the user can choose from.
    /// </summary>
    /// <param name="Level">The level, or <c>null</c> for "the minimum level of serilog.json".</param>
    /// <param name="Text">What the list shows.</param>
    public sealed record LogLevelChoice(LogLevel? Level, string Text)
    {
        /// <inheritdoc/>
        public override string ToString() => this.Text;
    }
}
