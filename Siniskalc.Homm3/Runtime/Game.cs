namespace Siniskalc.Homm3.Runtime
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Runtime;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A loaded savefile. Edits go through <see cref="AbstractSavegame.History"/>; <see cref="AbstractSavegame.Bytes"/> holds the loaded bytes.
    /// Loading and saving files is the job of <see cref="SavegameService{Game}"/>.
    /// </summary>
    public sealed class Game : HeroesSavegameBase<PlayerColor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <param name="file">The path of the savefile, or <see langword="null"/> when it was not read from a file.</param>
        /// <param name="bytes">The bytes of the savefile.</param>
        /// <param name="gameVersion">The version of the game that wrote the savefile.</param>
        /// <param name="history">The history of the savegame.</param>
        /// <param name="name">The name of the savegame, or <see langword="null"/> for none.</param>
        /// <param name="metadata">What the header of the savefile says.</param>
        /// <param name="places">Where the places a hero can visit are, or <see langword="null"/> for none.</param>
        public Game(string? file, byte[] bytes, GameVersion gameVersion, SaveDocumentHistory history, string? name, SaveMetadata<PlayerColor> metadata, MapPlaces? places = null)
            : base(file, bytes, history, name, metadata)
        {
            this.Places = places ?? MapPlaces.None;
            this.GameVersion = gameVersion;
        }

        /// <summary>
        /// Gets where the places a hero can visit are on the map.
        /// </summary>
        public MapPlaces Places { get; }

        /// <summary>
        /// Gets the version of the game that wrote the savefile.
        /// </summary>
        public GameVersion GameVersion { get; }

        /// <inheritdoc/>
        public override string VersionName => this.GameVersion switch
        {
            GameVersion.RestorationOfErathia => "Restoration of Erathia",
            GameVersion.ArmageddonsBlade => "Armageddon's Blade",
            GameVersion.ShadowOfDeath => "Shadow of Death",
            GameVersion.HeroesChronicles => "Heroes Chronicles",
            GameVersion.HornOfTheAbyss => "Horn of the Abyss",
            _ => "Unknown version",
        };

        /// <summary>
        /// Gets the colour whose turn it is (it can be changed), or <c>null</c> when it cannot be found or names no human.
        /// </summary>
        public override PlayerColor? CurrentPlayer => this.History.Current.Turn() is { Count: > 0 } turn && turn.GetModel(0).Color is var color && this.GetControl(color) == PlayerType.Human ? color : null;
    }
}
