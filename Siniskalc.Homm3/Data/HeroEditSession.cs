namespace Siniskalc.Homm3.Data
{
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A view onto a single hero in a <see cref="SaveDocumentHistory"/>. The hero keeps what depends on its artifacts (spells, skill
    /// bonuses) up to date itself, so its model is written as it is.
    /// </summary>
    public sealed class HeroEditSession : EditSession<HeroChunk, HeroModel>, IHeroSession
    {
        private static readonly ILogger<HeroEditSession> Logger = LogFactory.For<HeroEditSession>();

        public HeroEditSession(SaveDocumentHistory history, int heroIndex)
            : base(history, Homm3ChunkKinds.Hero, heroIndex, history.Current.Heroes().Deserializer, history.Current.Heroes().Serializer)
        {
            this.Offsets = history.Current.Heroes().Offsets;
        }

        public int HeroIndex => this.Index;

        /// <summary>
        /// Gets where the hero fields are in the chunk, for the savefile's game version.
        /// </summary>
        public HeroOffsets Offsets { get; }

        /// <inheritdoc/>
        public GameVersion GameVersion => this.History.Current.GameVersion();

        /// <inheritdoc/>
        public bool SupportsAsleep => this.History.Current.Heroes().SupportsAsleep;

        /// <inheritdoc/>
        public int GarrisonCastleIndex => this.History.Current.Castles().FindByGarrisonedHero(this.Current.Id);

        /// <inheritdoc/>
        public string CastleName(int castleIndex) => this.History.Current.Castles().GetName(castleIndex);

        /// <inheritdoc/>
        public IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory) => factory.ForHero(this);

        /// <summary>
        /// Gives the hero to a player, or to nobody, in one step of the history: the hero (<see cref="HeroModel.WithOwner"/>), the list of heroes of
        /// the player that had it and of the player that has it now, and the table of hero owners change together, as they do in the game
        /// (<see cref="HeroOwnerEdits.WithOwner"/>). Does nothing when the owner is the same.
        /// </summary>
        /// <param name="owner">The new owner, or <see cref="PlayerColor.Neutral"/>.</param>
        /// <exception cref="GameRuleException">The editor cannot make the change as the game would; the message says why, and nothing is changed.</exception>
        public void ChangeOwner(PlayerColor owner)
        {
            using var subject = Logger.BeginSubject("Hero", this.Current.Name);
            Logger.LogDebug("Changing the owner from {From} to {To}", this.Current.Owner, owner);
            var updated = HeroOwnerEdits.WithOwner(this.History.Current, this.HeroIndex, owner);
            if (!ReferenceEquals(updated, this.History.Current))
            {
                this.History.Apply(updated);
            }
        }

        /// <summary>
        /// Puts the owner of the hero back to what the savegame had, in one step of the history, with the lists of heroes of the players and the
        /// table of hero owners (<see cref="HeroOwnerEdits.WithOwnerAsIn"/>). It is how the change of the owner is reverted.
        /// </summary>
        /// <exception cref="GameRuleException">The hero goes back to a list of heroes that has become full.</exception>
        public void RestoreOwner()
        {
            using var subject = Logger.BeginSubject("Hero", this.Current.Name);
            Logger.LogDebug("Restoring the owner the savegame had");
            var updated = HeroOwnerEdits.WithOwnerAsIn(this.History.Current, this.History.Original, this.HeroIndex);
            if (!ReferenceEquals(updated, this.History.Current))
            {
                this.History.Apply(updated);
            }
        }
    }
}
