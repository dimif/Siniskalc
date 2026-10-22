namespace Siniskalc.Homm3.Runtime
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Runtime;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A loaded savefile. Edits go through <see cref="AbstractSavegame.History"/>; <see cref="AbstractSavegame.Bytes"/> holds the loaded bytes.
    /// Loading and saving files is the job of <see cref="GameService"/>.
    /// </summary>
    public sealed class Game : AbstractSavegame, IGameInfo<PlayerColor>
    {
        public Game(string? file, byte[] bytes, GameVersion gameVersion, SaveDocumentHistory history, string? name, SaveMetadata metadata, MapPlaces? places = null)
            : base(file, bytes, history, name)
        {
            this.Places = places ?? MapPlaces.None;
            this.GameVersion = gameVersion;
            this.Metadata = metadata;
        }

        /// <summary>
        /// Gets what the savefile says about the game besides its editable structs.
        /// </summary>
        public SaveMetadata Metadata { get; }

        /// <summary>
        /// Gets where the places a hero can visit are on the map.
        /// </summary>
        public MapPlaces Places { get; }

        public GameVersion GameVersion { get; }

        /// <inheritdoc/>
        public override string VersionName => this.GameVersion.ToString();

        public bool IsHota => this.GameVersion == GameVersion.HornOfTheAbyss;

        /// <summary>
        /// Gets the date in the game when the savefile was loaded, or <c>null</c> when it cannot be found in this version.
        /// </summary>
        public GameDate? Date => this.Metadata.Date;

        /// <summary>
        /// Gets who plays each colour, indexed by <see cref="PlayerColor"/>, or <c>null</c> when it cannot be found in this version.
        /// </summary>
        public IReadOnlyList<PlayerType>? PlayerControls => this.Metadata.Setup?.PlayerControls;

        /// <summary>
        /// Gets the difficulty the game was started with, or <c>null</c> when it cannot be found in this version.
        /// </summary>
        public GameDifficulty? Difficulty => this.Metadata.Setup?.Difficulty;

        /// <summary>
        /// Gets the map's name, description and difficulty, or <c>null</c> when they cannot be found in this version.
        /// </summary>
        public MapHeader? Map => this.Metadata.Map;

        /// <summary>
        /// Gets the difficulty the map was made with, which is set when the map is created and is not the difficulty the game was
        /// started at, or <c>null</c> when it cannot be found in this version.
        /// </summary>
        public GameDifficulty? MapDifficulty => this.Map?.Difficulty;

        /// <summary>
        /// Gets the colour whose turn it was when the savefile was made, or <c>null</c> when it cannot be found.
        /// </summary>
        public PlayerColor? CurrentPlayer => this.Metadata.Setup?.CurrentPlayer;

        /// <summary>
        /// Returns who plays <paramref name="color"/>, or <c>null</c> when that is not known.
        /// </summary>
        public PlayerType? GetControl(PlayerColor color)
        {
            var index = color.Code;
            return this.PlayerControls != null && index < this.PlayerControls.Count ? this.PlayerControls[index] : null;
        }
    }
}
