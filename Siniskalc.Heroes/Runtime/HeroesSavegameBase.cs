namespace Siniskalc.Heroes.Runtime
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// A loaded savefile of any game of the series: what <see cref="AbstractSavegame"/> has, and what the savefile says about the game
    /// (<see cref="SaveMetadata{TColor}"/>), which is <see cref="IGameInfo{TColor}"/> written once. A game adds its version and its places.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    public abstract class HeroesSavegameBase<TColor> : AbstractSavegame, IGameInfo<TColor>
        where TColor : class, IPlayerColor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeroesSavegameBase{TColor}"/> class.
        /// </summary>
        /// <param name="fileName">The file the savefile was loaded from, or <c>null</c>.</param>
        /// <param name="bytes">The bytes as loaded.</param>
        /// <param name="history">The history of the edits.</param>
        /// <param name="name">The name to show, or <c>null</c> for the name of the file.</param>
        /// <param name="metadata">What the savefile says about the game.</param>
        protected HeroesSavegameBase(string? fileName, byte[] bytes, SaveDocumentHistory history, string? name, SaveMetadata<TColor> metadata)
            : base(fileName, bytes, history, name)
        {
            this.Metadata = metadata;
        }

        /// <summary>
        /// Gets what the savefile says about the game besides its records.
        /// </summary>
        public SaveMetadata<TColor> Metadata { get; }

        /// <inheritdoc/>
        public GameDate? Date => this.Metadata.Date;

        /// <inheritdoc/>
        public MapHeader? Map => this.Metadata.Map;

        /// <inheritdoc/>
        public GameDifficulty? Difficulty => this.Metadata.Setup?.Difficulty;

        /// <inheritdoc/>
        public virtual TColor? CurrentPlayer => this.Metadata.Setup?.CurrentPlayer;

        /// <inheritdoc/>
        public PlayerType? GetControl(TColor color) => this.Metadata.Setup?.GetControl(color);
    }
}
