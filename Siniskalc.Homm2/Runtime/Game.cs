namespace Siniskalc.Homm2.Runtime
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Runtime;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// A loaded Heroes of Might and Magic II savefile. Edits go through <see cref="AbstractSavegame.History"/>; <see cref="AbstractSavegame.Bytes"/>
    /// holds the loaded bytes.
    /// </summary>
    public sealed class Game : HeroesSavegameBase<PlayerColor>
    {
        public Game(string? file, byte[] bytes, SaveLayout layout, SaveDocumentHistory history, SaveMetadata<PlayerColor> metadata, string? name)
            : base(file, bytes, history, name, metadata)
        {
            this.Layout = layout;
        }

        public SaveLayout Layout { get; }

        /// <summary>
        /// Gets where the places a hero can visit once are on the map.
        /// </summary>
        public MapPlaces Places { get; init; } = MapPlaces.None;

        public GameVersion GameVersion => this.Layout.Version;

        /// <inheritdoc/>
        public override string VersionName => this.GameVersion.ToString();

        /// <summary>
        /// Gets the colour whose turn it is (it can be changed), or <c>null</c> when the savegame does not say.
        /// </summary>
        public override PlayerColor? CurrentPlayer => this.History.Current.Turn().GetModel(0) is { Color: { } color } && this.GetControl(color) == PlayerType.Human ? color : null;

        /// <summary>
        /// Gets the number of the record of a player in the table of players, which is what the savegame keeps of whose turn it is.
        /// </summary>
        public int RecordOf(PlayerColor color) => (this.History.Current.Players()[color.Code].StartIndex - this.Layout.PlayerTableStart) / PlayerLayout.RecordSize;
    }
}
