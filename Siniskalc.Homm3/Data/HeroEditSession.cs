namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A view onto a single hero in a <see cref="SaveDocumentHistory"/>. The hero keeps what depends on its artifacts (spells, skill
    /// bonuses) up to date itself, so its model is written as it is.
    /// </summary>
    public sealed class HeroEditSession : EditSession<HeroChunk, HeroModel>, IHeroSession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeroEditSession"/> class.
        /// </summary>
        /// <param name="history">The history.</param>
        /// <param name="heroIndex">The hero index.</param>
        public HeroEditSession(SaveDocumentHistory history, int heroIndex)
            : base(history, Homm3ChunkKinds.Hero, heroIndex, history.Current.Heroes().Deserializer, history.Current.Heroes().Serializer)
        {
            this.Offsets = history.Current.Heroes().Offsets;
        }

        /// <summary>
        /// Gets the number of the hero's record.
        /// </summary>
        public int HeroIndex => this.Index;

        /// <summary>
        /// Gets where the hero fields are in the chunk, for the savefile's game version.
        /// </summary>
        public HeroOffsets Offsets { get; }

        /// <inheritdoc/>
        public IReadOnlyList<PlayerColor> PlayersInGame => this.History.Current.Players().GetColors();

        /// <inheritdoc/>
        public GameVersion GameVersion => this.History.Current.GameVersion();

        /// <inheritdoc/>
        public bool SupportsAsleep => this.History.Current.Heroes().SupportsAsleep;

        /// <inheritdoc/>
        public bool SupportsMovedToday => this.History.Current.Heroes().SupportsMovedToday;

        /// <inheritdoc/>
        public int GarrisonCastleIndex => this.History.Current.Castles().FindByGarrisonedHero(this.Current.Id);

        /// <inheritdoc/>
        public string CastleName(int castleIndex) => this.History.Current.Castles().GetName(castleIndex);

        /// <inheritdoc/>
        public IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory) => factory.ForHero(this);
    }
}
