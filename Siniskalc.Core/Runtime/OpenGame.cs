namespace Siniskalc.Core.Runtime
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// What every open savefile has, whatever the game: the file it is tied to, its name, the loaded bytes and the history of edits.
    /// A game adds what its savefile says about the game.
    /// </summary>
    public abstract class OpenGame : IOpenGame
    {
        protected OpenGame(string? file, byte[] bytes, SaveDocumentHistory history, string? name = null)
        {
            this.File = file;
            this.Name = name ?? file;
            this.Bytes = bytes;
            this.History = history;
        }

        /// <inheritdoc/>
        public string? File { get; private set; }

        /// <inheritdoc/>
        public string? Name { get; }

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
            this.File = filePath;
        }

        /// <inheritdoc/>
        public byte[] GetBytesToSave() => this.History.Current.ApplyChangesTo(this.Bytes);
    }
}
