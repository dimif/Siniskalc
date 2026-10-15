namespace Siniskalc.Core.Runtime
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// What a game's editor needs to know about the savefile it has open, whatever the game: where it is, what it is called, its history
    /// of edits and the bytes to save.
    /// </summary>
    public interface IOpenGame
    {
        /// <summary>
        /// Gets the savefile the game is tied to: where it was loaded from, or last saved to. <c>null</c> when it was loaded from bytes.
        /// </summary>
        string? File { get; }

        /// <summary>
        /// Gets what to call the game: its file, or the name it was given when loaded from bytes.
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// Gets the name of the game version of the savefile, such as "RestorationOfErathia".
        /// </summary>
        string VersionName { get; }

        /// <summary>
        /// Gets the undo and redo history of edits, shared by everything in the savefile.
        /// </summary>
        SaveDocumentHistory History { get; }

        /// <summary>
        /// Returns the bytes to save: the loaded bytes with every edit from <see cref="History"/> applied.
        /// </summary>
        byte[] GetBytesToSave();

        /// <summary>
        /// Ties the game to the file it was just saved to, which is what reloading reads.
        /// </summary>
        void SavedTo(string filePath);
    }
}
