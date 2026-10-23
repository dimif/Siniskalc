namespace Siniskalc.Homm3.Runtime
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Runtime;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A loaded savefile. Edits go through <see cref="AbstractSavegame.History"/>; <see cref="AbstractSavegame.Bytes"/> holds the loaded bytes.
    /// Loading and saving files is the job of <see cref="GameService"/>.
    /// </summary>
    public sealed class Game : HeroesSavegameBase<PlayerColor>
    {
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

        public GameVersion GameVersion { get; }

        /// <inheritdoc/>
        public override string VersionName => this.GameVersion.ToString();

        public bool IsHota => this.GameVersion == GameVersion.HornOfTheAbyss;

        /// <summary>
        /// Gets who plays each colour, indexed by <see cref="PlayerColor"/>, or <c>null</c> when it cannot be found in this version.
        /// </summary>
        public IReadOnlyList<PlayerType>? PlayerControls => this.Metadata.Setup?.PlayerControls;

        /// <summary>
        /// Gets the difficulty the map was made with, which is set when the map is created and is not the difficulty the game was
        /// started at, or <c>null</c> when it cannot be found in this version.
        /// </summary>
        public GameDifficulty? MapDifficulty => this.Map?.Difficulty;

    }
}
