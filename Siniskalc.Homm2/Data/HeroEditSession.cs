namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Edits one hero of a savegame through its history.
    /// </summary>
    public sealed class HeroEditSession : EditSession<HeroChunk, HeroModel>
    {
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
    }
}
