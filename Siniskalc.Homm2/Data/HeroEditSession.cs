namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Edits one hero of a savegame through its history.
    /// </summary>
    public sealed class HeroEditSession : EditSession<HeroChunk, HeroModel>
    {
        private static readonly ILogger<HeroEditSession> Logger = LogFactory.For<HeroEditSession>();

        public HeroEditSession(SaveDocumentHistory history, int heroIndex)
            : base(history, Homm2ChunkKinds.Hero, heroIndex, history.Current.Heroes().Deserializer, history.Current.Heroes().Serializer)
        {
        }

        public int HeroIndex => this.Index;

        /// <summary>
        /// Gives the hero to a player, or to neutral (nobody), in one step of the history. The hero, the list of heroes of the player that had it and
        /// the list of the player that has it now change together, as they do in the game. A hero that serves nobody has no move to finish,
        /// as a defeated one has none (the Broken Alliance saves 15 and 16). Does nothing when the owner is the same.
        /// </summary>
        /// <exception cref="GameRuleException">The new owner has the most heroes the list holds.</exception>
        public void ChangeOwner(PlayerColor owner)
        {
            var document = this.History.Current;
            var heroes = document.Heroes();
            var players = document.Players();
            var hero = this.Current;
            if (hero.Owner == owner)
            {
                return;
            }

            using var subject = Logger.BeginSubject("Hero", hero.Name);
            Logger.LogDebug("Changing the owner from {From} to {To}", hero.Owner, owner);

            // The new lists are made before anything is written, so a full list leaves the savegame as it was.
            var edits = new List<(int Player, PlayerModel Model)>();
            if (!hero.Owner.IsNeutral)
            {
                edits.Add((hero.Owner.Code, players.GetModel(hero.Owner.Code).WithoutHero(hero.Id)));
            }

            if (!owner.IsNeutral)
            {
                edits.Add((owner.Code, players.GetModel(owner.Code).WithHero(hero.Id)));
            }

            var changed = hero.WithOwner(owner);
            var updated = document.WithChunk(Homm2ChunkKinds.Hero, this.HeroIndex, heroes.Serializer.Serialize(heroes[this.HeroIndex], changed));
            foreach (var (player, model) in edits)
            {
                updated = updated.WithChunk(Homm2ChunkKinds.Player, player, players.Serializer.Serialize(players[player], model));
            }

            this.History.Apply(updated);
        }

        /// <summary>
        /// Puts the owner of the hero back to what the savegame had, in one step of the history, with the lists of heroes of the players
        /// (<see cref="PlayerModel.WithHeroAsIn"/>). It is how the change of the owner is reverted. The rest of the hero (such as the move it has
        /// left) is a change of its own and is left as it is.
        /// </summary>
        /// <exception cref="GameRuleException">The hero goes back to a list of heroes that has become full.</exception>
        public void RestoreOwner()
        {
            using var subject = Logger.BeginSubject("Hero", this.Current.Name);
            Logger.LogDebug("Restoring the owner the savegame had");
            var updated = WithOwnerAsIn(this.History.Current, this.History.Original, this.HeroIndex);
            if (!ReferenceEquals(updated, this.History.Current))
            {
                this.History.Apply(updated);
            }
        }

        /// <summary>
        /// Returns the document with the owner of the hero as it is in <paramref name="original"/>, in the hero and in the lists of heroes of the
        /// players. The new records are all made before any is written, so a refused change leaves the document as it was.
        /// </summary>
        /// <exception cref="GameRuleException">The hero goes back to a list of heroes that has become full.</exception>
        internal static SaveDocument WithOwnerAsIn(SaveDocument document, SaveDocument original, int heroIndex)
        {
            var heroes = document.Heroes();
            var hero = heroes.GetModel(heroIndex);
            var originalHero = original.Heroes().GetModel(heroIndex);

            var players = document.Players();
            var originalPlayers = original.Players();
            var edits = new List<(int Player, PlayerModel Model)>();
            for (var index = 0; index < players.Count && index < originalPlayers.Count; index++)
            {
                var player = players.GetModel(index);
                var originalPlayer = originalPlayers.GetModel(index);
                if (player.HeroIds.Contains(hero.Id) || originalPlayer.HeroIds.Contains(hero.Id))
                {
                    var restored = player.WithHeroAsIn(hero.Id, originalPlayer);
                    if (restored != player)
                    {
                        edits.Add((index, restored));
                    }
                }
            }

            var updated = hero.Owner == originalHero.Owner
                ? document
                : document.WithChunk(Homm2ChunkKinds.Hero, heroIndex, heroes.Serializer.Serialize(heroes[heroIndex], hero with { Owner = originalHero.Owner }));
            foreach (var (player, model) in edits)
            {
                updated = updated.WithChunk(Homm2ChunkKinds.Player, player, players.Serializer.Serialize(players[player], model));
            }

            return updated;
        }
    }
}
