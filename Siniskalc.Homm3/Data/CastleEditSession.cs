namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A view onto a single castle in a <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class CastleEditSession : EditSession<CastleChunk, CastleModel>, ICastleSession
    {
        public CastleEditSession(SaveDocumentHistory history, int castleIndex)
            : base(history, Homm3ChunkKinds.Castle, castleIndex, history.Current.Castles().Deserializer, history.Current.Castles().Serializer)
        {
        }

        public int CastleIndex => this.Index;

        /// <inheritdoc/>
        public string Name => this.History.Current.Castles().GetName(this.CastleIndex);

        /// <inheritdoc/>
        public GameVersion GameVersion => this.History.Current.GameVersion();

        /// <inheritdoc/>
        public IHeroSummaries<PlayerColor> Heroes => this.History.Current.Heroes();

        /// <inheritdoc/>
        public HeroModel? FindHero(int heroId)
        {
            var heroes = this.History.Current.Heroes();
            var index = heroes.IndexOfId(heroId);
            return index < 0 ? null : heroes.GetModel(index);
        }

        /// <inheritdoc/>
        public IByteEditTarget CreateByteEditTarget(IByteEditTargetFactory factory) => factory.ForCastle(this);

        /// <summary>
        /// Gives the castle to a player, or to nobody, in one step of the history. The castle and the countdowns of the player that had it and of
        /// the player that has it now change together, as they do in the game: the player who loses its last town has
        /// <see cref="Constants.Player.MaxDaysLeft"/> days left, and the one who gains a town stops counting (<see cref="PlayerModel.WithTownLost"/>,
        /// <see cref="PlayerModel.WithTownGained"/>). The towns a player owns are the castles whose owner is its colour. Does nothing when the
        /// owner is the same.
        /// </summary>
        /// <param name="owner">The new owner, or <see cref="PlayerColor.Neutral"/>.</param>
        public void ChangeOwner(PlayerColor owner)
        {
            var document = this.History.Current;
            var castles = document.Castles();
            var castle = this.Current;
            if (castle.Owner == owner)
            {
                return;
            }

            // The towns of the player that had the castle, counted from the castles, without this one.
            var townsLeft = castles.GetOwners().Count(o => o == castle.Owner) - 1;
            var updated = document.WithCastle(this.CastleIndex, castles.Serializer.Serialize(castles[this.CastleIndex], castle with { Owner = owner }));
            updated = WithPlayer(updated, castle.Owner, player => player.WithTownLost(townsLeft));
            updated = WithPlayer(updated, owner, player => player.WithTownGained());
            this.History.Apply(updated);
        }

        // Nobody (Neutral) has no player record, and a savefile whose players are not known has none at all: there is nothing to change then.
        private static SaveDocument WithPlayer(SaveDocument document, PlayerColor color, Func<PlayerModel, PlayerModel> edit)
        {
            var players = document.Players();
            var index = color.IsNeutral ? -1 : players.FindByColor(color);
            if (index < 0)
            {
                return document;
            }

            var before = players.GetModel(index);
            var after = edit(before);
            return after == before ? document : document.WithPlayer(index, players.Serializer.Serialize(players[index], after));
        }
    }
}
