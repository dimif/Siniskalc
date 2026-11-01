namespace Siniskalc.Core.Runtime
{
    using System;

    /// <summary>
    /// Standard game definitions cannot be loaded or validated, so the editor must not start.
    /// </summary>
    public sealed class GameDefinitionException : Exception
    {
        public GameDefinitionException(string path, string message, Exception? innerException = null)
            : base($"Invalid game definitions in '{path}': {message}", innerException)
        {
            this.Path = path;
        }

        public string Path { get; }
    }
}
