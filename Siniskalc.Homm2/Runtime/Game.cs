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
        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="bytes">The bytes.</param>
        /// <param name="layout">The layout.</param>
        /// <param name="history">The history.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="name">The name.</param>
        public Game(string? file, byte[] bytes, SaveLayout layout, SaveDocumentHistory history, SaveMetadata<PlayerColor> metadata, string? name)
            : base(file, bytes, history, name, metadata)
        {
            this.Layout = layout;
        }

        /// <summary>
        /// Gets where the tables are in the savegame.
        /// </summary>
        public SaveLayout Layout { get; }

        /// <summary>
        /// Gets the catalog of the version of the game the savegame was made with.
        /// </summary>
        public GameCatalog Catalog { get; init; } = null!;

        /// <summary>
        /// Gets where the places a hero can visit once are on the map.
        /// </summary>
        public MapPlaces<PlaceKind> Places { get; init; } = MapPlaces<PlaceKind>.None;

        /// <summary>
        /// Gets the version of the game the savegame was made with.
        /// </summary>
        public GameVersion GameVersion => this.Layout.Version;

        /// <inheritdoc/>
        public override string VersionName => this.GameVersion == GameVersion.PriceOfLoyalty ? "Price of Loyalty" : "The Succession Wars";

        /// <summary>
        /// Gets the colour whose turn it is (it can be changed), or <c>null</c> when the savegame does not say.
        /// </summary>
        public override PlayerColor? CurrentPlayer => this.History.Current.Turn().GetModel(0) is { Color: { } color } && this.GetControl(color) == PlayerType.Human ? color : null;

        /// <summary>
        /// Gets the number of the record of a player in the table of players, which is what the savegame keeps of whose turn it is.
        /// </summary>
        /// <param name="color">The colour.</param>
        /// <returns>The number of the record, from 0.</returns>
        public int RecordOf(PlayerColor color) => (this.History.Current.Players()[color.Code].StartIndex - this.Layout.PlayerTableStart) / PlayerLayout.RecordSize;
    }
}
