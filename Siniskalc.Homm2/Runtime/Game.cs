namespace Siniskalc.Homm2.Runtime
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// A loaded Heroes of Might and Magic II savefile. Edits go through <see cref="OpenGame.History"/>; <see cref="OpenGame.Bytes"/>
    /// holds the loaded bytes.
    /// </summary>
    public sealed class Game : OpenGame
    {
        public Game(string? file, byte[] bytes, SaveLayout layout, SaveDocumentHistory history, SaveMetadata metadata, string? name = null)
            : base(file, bytes, history, name)
        {
            this.Layout = layout;
            this.Metadata = metadata;
        }

        public SaveLayout Layout { get; }

        public GameVersion GameVersion => this.Layout.Version;

        /// <inheritdoc/>
        public override string VersionName => this.GameVersion.ToString();

        public SaveMetadata Metadata { get; }

        /// <summary>
        /// Gets the date in the game when the savefile was made.
        /// </summary>
        public GameDate Date => this.Metadata.Date;

        public MapHeader Map => this.Metadata.Map;
    }
}
