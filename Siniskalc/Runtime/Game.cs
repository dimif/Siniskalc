namespace Siniskalc.Runtime
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// A loaded savefile. Edits go through <see cref="History"/>; <see cref="Bytes"/> holds the loaded bytes.
    /// Loading and saving files is the job of <see cref="GameService"/>.
    /// </summary>
    public class Game
    {
        public Game(string? file, byte[] bytes, GameVersion gameVersion, SaveDocumentHistory history, SaveMetadata? metadata = null, string? name = null)
        {
            this.File = file;
            this.Name = name ?? file;
            this.Bytes = bytes;
            this.GameVersion = gameVersion;
            this.History = history;
            this.Metadata = metadata ?? SaveMetadata.None;
        }

        /// <summary>
        /// Gets what the savefile says about the game besides its editable structs.
        /// </summary>
        public SaveMetadata Metadata { get; }

        /// <summary>
        /// Gets the savefile the game is tied to: where it was loaded from, or last saved to. <c>null</c> when it was loaded from bytes
        /// (a dump), which have no file to read again.
        /// </summary>
        public string? File { get; private set; }

        /// <summary>
        /// Gets what to call the game: its file, or the name it was given when loaded from bytes. <c>null</c> when it has neither.
        /// </summary>
        public string? Name { get; }

        public GameVersion GameVersion { get; }

        public bool IsHota => this.GameVersion == GameVersion.HornOfTheAbyss;

        /// <summary>
        /// Gets the savefile bytes as loaded; saving applies the edits from <see cref="History"/> to them.
        /// </summary>
        public byte[] Bytes { get; }

        /// <summary>
        /// Gets the undo/redo history of edits, shared by every hero and player in this savefile.
        /// </summary>
        public SaveDocumentHistory History { get; }

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
            var index = (int)color;
            return this.PlayerControls != null && index < this.PlayerControls.Count ? this.PlayerControls[index] : null;
        }

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
        public byte[] GetBytesToSave()
        {
            return this.History.Current.ApplyChangesTo(this.Bytes);
        }
    }
}
