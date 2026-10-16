namespace Siniskalc.Core.Runtime
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// What every open savefile has, whatever the game: the file it is tied to, its name, the loaded bytes and the history of edits.
    /// A game adds what its savefile says about the game.
    /// </summary>
    public abstract class AbstractSavegame : ISavegame
    {
        private const string UnnamedGame = "Savefile";

        protected AbstractSavegame(string? fileName, byte[] bytes, SaveDocumentHistory history, string? name)
        {
            this.FileName = fileName;
            this.Name = name ?? fileName ?? UnnamedGame;
            this.Bytes = bytes;
            this.History = history;
        }

        /// <inheritdoc/>
        public string? FileName { get; private set; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Gets the savefile bytes as loaded; saving applies the edits from <see cref="History"/> to them.
        /// </summary>
        public byte[] Bytes { get; }

        /// <inheritdoc/>
        public abstract string VersionName { get; }

        /// <inheritdoc/>
        public SaveDocumentHistory History { get; }

        /// <inheritdoc/>
        public void SavedTo(string filePath)
        {
            this.FileName = filePath;
        }

        /// <inheritdoc/>
        public byte[] GetBytesToSave() => this.History.Current.ApplyChangesTo(this.Bytes);
    }
}
