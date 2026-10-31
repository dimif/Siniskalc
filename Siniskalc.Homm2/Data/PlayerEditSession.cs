namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Edits one player of a savegame through its history.
    /// </summary>
    public sealed class PlayerEditSession : EditSession<PlayerChunk, PlayerModel>
    {
        private static readonly ILogger<PlayerEditSession> Logger = LogFactory.For<PlayerEditSession>();

        public PlayerEditSession(SaveDocumentHistory history, int playerIndex)
            : base(history, Homm2ChunkKinds.Player, playerIndex, history.Current.Players().Deserializer, history.Current.Players().Serializer)
        {
        }

        public int PlayerIndex => this.Index;

        /// <summary>
        /// Puts the list of heroes and the selected hero of the player back to what the savegame had, in one step of the history. The list only
        /// changes with the owner of a hero, so each hero that joined or left it gets its owner back, with the other lists
        /// (<see cref="HeroEditSession.WithOwnerAsIn"/>); what is still different then (the order, the selected hero) is put back in the player alone.
        /// </summary>
        /// <exception cref="GameRuleException">A hero goes back to a list of heroes that has become full.</exception>
        public void RestoreHeroes()
        {
            using var subject = Logger.BeginSubject("Player", this.Chunk.Name);
            Logger.LogDebug("Restoring the list of heroes the savegame had");
            var document = this.History.Current;
            var original = this.History.Original;
            var current = this.Current;
            var loaded = original.Players().GetModel(this.PlayerIndex);
            var heroes = document.Heroes().Count;
            foreach (var heroId in current.HeroIds.Except(loaded.HeroIds).Concat(loaded.HeroIds.Except(current.HeroIds)).Where(id => id >= 0 && id < heroes).ToList())
            {
                // The number of a hero is its place among the heroes.
                document = HeroEditSession.WithOwnerAsIn(document, original, heroId);
            }

            this.Apply(document, player => player with { HeroIds = loaded.HeroIds, SelectedHero = loaded.SelectedHero });
        }

        /// <summary>
        /// Puts the list of castles, the selected castle and the days left to own a castle of the player back to what the savegame had, in one
        /// step of the history. The list only changes with the owner of a castle, so each castle that joined or left it gets its owner back, with
        /// the other lists (<see cref="CastleEditSession.WithOwnerAsIn"/>); what is still different then (the order, the selected castle) is put
        /// back in the player alone.
        /// </summary>
        /// <exception cref="GameRuleException">A castle goes back to a list of castles that has become full.</exception>
        public void RestoreCastles()
        {
            using var subject = Logger.BeginSubject("Player", this.Chunk.Name);
            Logger.LogDebug("Restoring the list of castles the savegame had");
            var document = this.History.Current;
            var original = this.History.Original;
            var current = this.Current;
            var loaded = original.Players().GetModel(this.PlayerIndex);
            var castles = document.Castles().Count;
            foreach (var castleIndex in current.CastleIds.Except(loaded.CastleIds).Concat(loaded.CastleIds.Except(current.CastleIds)).Where(id => id >= 0 && id < castles).ToList())
            {
                // The number of a castle is its place among the castles.
                document = CastleEditSession.WithOwnerAsIn(document, original, castleIndex);
            }

            this.Apply(document, player => player with { CastleIds = loaded.CastleIds, SelectedCastle = loaded.SelectedCastle });
        }

        private void Apply(SaveDocument document, Func<PlayerModel, PlayerModel> restore)
        {
            var players = document.Players();
            var player = players.GetModel(this.PlayerIndex);
            var restored = restore(player);
            if (restored != player)
            {
                document = document.WithChunk(Homm2ChunkKinds.Player, this.PlayerIndex, players.Serializer.Serialize(players[this.PlayerIndex], restored));
            }

            if (!ReferenceEquals(document, this.History.Current))
            {
                this.History.Apply(document);
            }
        }
    }
}
