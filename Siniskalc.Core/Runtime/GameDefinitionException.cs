namespace Siniskalc.Core.Runtime
{
    using System;

    /// <summary>
    /// Standard game definitions cannot be loaded or validated, so the editor must not start.
    /// </summary>
    public sealed class GameDefinitionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameDefinitionException"/> class.
        /// </summary>
        /// <param name="path">The definition file, relative to the game data folder.</param>
        /// <param name="message">What is wrong with it; the message of the exception names the file as well.</param>
        /// <param name="innerException">The failure behind it, if any.</param>
        public GameDefinitionException(string path, string message, Exception? innerException = null)
            : base($"Invalid game definitions in '{path}': {message}", innerException)
        {
            this.Path = path;
        }

        /// <summary>
        /// Gets the definition file the failure is about, relative to the game data folder.
        /// </summary>
        public string Path { get; }
    }
}
