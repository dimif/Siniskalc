namespace Siniskalc.Core.Runtime
{
    /// <summary>
    /// The file is readable but is not a savefile this editor knows how to edit (another game, a map, or a version it does not know).
    /// </summary>
    public sealed class UnsupportedSavefileException : SavefileException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedSavefileException"/> class.
        /// </summary>
        /// <param name="message">Why the file is not supported, for the user.</param>
        public UnsupportedSavefileException(string message)
            : base(message)
        {
        }
    }
}
