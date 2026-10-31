namespace Siniskalc.Core.Runtime
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// What every open savefile has, whatever the game: the file it is tied to, its name, the loaded bytes and the history of edits.
    /// A game adds what its savefile says about the game.
    /// </summary>
    /// <param name="fileName">The file the savefile was loaded from, or <see langword="null"/> when it came from bytes.</param>
    /// <param name="bytes">The savefile bytes as loaded.</param>
    /// <param name="history">The history of edits of the savefile.</param>
    /// <param name="name">What to call the savefile; the file name when omitted, else a generic name.</param>
    public abstract class AbstractSavegame(string? fileName, byte[] bytes, SaveDocumentHistory history, string? name) : ISavegame
    {
        private const string UnnamedGame = "Savefile";

        /// <inheritdoc/>
        public string? FileName { get; private set; } = fileName;

        /// <inheritdoc/>
        public string Name { get; } = name ?? fileName ?? UnnamedGame;

        /// <summary>
        /// Gets the savefile bytes as loaded; saving applies the edits from <see cref="History"/> to them.
        /// </summary>
        public byte[] Bytes { get; } = bytes;

        /// <inheritdoc/>
        public abstract string VersionName { get; }

        /// <inheritdoc/>
        public SaveDocumentHistory History { get; } = history;

        /// <inheritdoc/>
        public void SavedTo(string filePath) => this.FileName = filePath;

        /// <inheritdoc/>
        public byte[] GetBytesToSave() => this.History.Current.ApplyChangesTo(this.Bytes);
    }
}
