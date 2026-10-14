namespace Siniskalc.Homm2.Runtime
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// A loaded Heroes of Might and Magic II savefile. Edits go through <see cref="History"/>; <see cref="Bytes"/> holds the loaded bytes.
    /// </summary>
    public sealed class Game : Siniskalc.Core.Runtime.IOpenGame
    {
        public Game(string? file, byte[] bytes, SaveLayout layout, SaveDocumentHistory history, SaveMetadata metadata, string? name = null)
        {
            this.File = file;
            this.Name = name ?? file;
            this.Bytes = bytes;
            this.Layout = layout;
            this.History = history;
            this.Metadata = metadata;
        }

        /// <summary>
        /// Gets the savefile the game is tied to: where it was loaded from, or last saved to. <c>null</c> when it was loaded from bytes.
        /// </summary>
        public string? File { get; private set; }

        /// <summary>
        /// Gets what to call the game: its file, or the name it was given when loaded from bytes.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the savefile bytes as loaded; saving applies the edits from <see cref="History"/> to them.
        /// </summary>
        public byte[] Bytes { get; }

        public SaveLayout Layout { get; }

        public GameVersion GameVersion => this.Layout.Version;

        /// <inheritdoc/>
        public string VersionName => this.GameVersion.ToString();

        /// <summary>
        /// Gets the undo and redo history of edits, shared by every hero, player and castle of the savefile.
        /// </summary>
        public SaveDocumentHistory History { get; }

        public SaveMetadata Metadata { get; }

        /// <summary>
        /// Gets the date in the game when the savefile was made.
        /// </summary>
        public GameDate Date => this.Metadata.Date;

        public MapHeader Map => this.Metadata.Map;

        /// <summary>
        /// Ties the game to the file it was just saved to, which is what reloading reads.
        /// </summary>
        public void SavedTo(string filePath)
        {
            this.File = filePath;
        }

        /// <summary>
        /// Returns the bytes to save: the loaded bytes with every edit from <see cref="History"/> applied.
        /// </summary>
        public byte[] GetBytesToSave() => this.History.Current.ApplyChangesTo(this.Bytes);
    }
}
