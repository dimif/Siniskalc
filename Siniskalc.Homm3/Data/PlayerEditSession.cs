namespace Siniskalc.Homm3.Data
{
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A view onto a single player in a <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class PlayerEditSession : EditSession<PlayerChunk, PlayerModel>
    {
        private static readonly ILogger<PlayerEditSession> Logger = LogFactory.For<PlayerEditSession>();

        public PlayerEditSession(SaveDocumentHistory history, int playerIndex)
            : base(history, Homm3ChunkKinds.Player, playerIndex, history.Current.Players().Deserializer, history.Current.Players().Serializer)
        {
        }

        public int PlayerIndex => this.Index;

        /// <summary>
        /// Puts the list of heroes and the selected hero of the player back to what the savegame had, in one step of the history. The list only
        /// changes with the owner of a hero, so each hero that joined or left it gets its owner back, with the other lists and the table of hero
        /// owners (<see cref="HeroOwnerEdits.WithOwnerAsIn"/>); what is still different then (the order, the selected hero) is put back in the
        /// player alone.
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
            var heroes = document.Heroes();
            foreach (var heroId in current.HeroIds.Except(loaded.HeroIds).Concat(loaded.HeroIds.Except(current.HeroIds)).ToList())
            {
                var heroIndex = heroes.IndexOfId(heroId);
                if (heroIndex >= 0)
                {
                    document = HeroOwnerEdits.WithOwnerAsIn(document, original, heroIndex);
                }
            }

            var players = document.Players();
            var player = players.GetModel(this.PlayerIndex);
            var restored = player with { HeroIds = loaded.HeroIds, SelectedHero = loaded.SelectedHero };
            if (restored != player)
            {
                document = document.WithPlayer(this.PlayerIndex, players.Serializer.Serialize(players[this.PlayerIndex], restored));
            }

            if (!ReferenceEquals(document, this.History.Current))
            {
                this.History.Apply(document);
            }
        }
    }
}
