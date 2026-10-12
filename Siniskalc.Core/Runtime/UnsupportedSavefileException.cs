namespace Siniskalc.Core.Runtime
{
    /// <summary>
    /// The file is readable but is not a savefile this editor knows how to edit (another game, a map, or a version it does not know).
    /// </summary>
    public sealed class UnsupportedSavefileException : SavefileException
    {
        public UnsupportedSavefileException(string message)
            : base(message)
        {
        }
    }
}
