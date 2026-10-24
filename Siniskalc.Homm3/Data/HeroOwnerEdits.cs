namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Gives a hero to another owner across the records that say who owns it: the hero, the lists of heroes of the players
    /// (<see cref="PlayerModel.HeroIds"/>) and the table of hero owners (<see cref="HeroOwnersModel"/>), as the game keeps them in step
    /// (<c>TestData/Hota/HeroOwners</c>). Pure functions of documents; <see cref="HeroEditSession"/> and <see cref="PlayerEditSession"/> apply
    /// what they return as one step of the history.
    /// </summary>
    public static class HeroOwnerEdits
    {
        /// <summary>
        /// Returns the document with the hero given to <paramref name="owner"/>, or to nobody. The player who had it loses it from its list
        /// (<see cref="PlayerModel.WithoutHero"/>), the player who has it now gains it at the end of its list (<see cref="PlayerModel.WithHero"/>),
        /// and the table of hero owners says the new owner. A player with no record (Neutral, or a savefile whose players are not known) and a
        /// savefile with no table are skipped. The new records are all made before any is written, so a refused change leaves the document as it was.
        /// </summary>
        /// <exception cref="GameRuleException">
        /// The change is one the editor cannot make as the game would: the hero is garrisoned in a castle, the new owner's list is full, or the
        /// hero is offered in a tavern or kept out of play by the game.
        /// </exception>
        public static SaveDocument WithOwner(SaveDocument document, int heroIndex, PlayerColor owner)
        {
            var heroes = document.Heroes();
            var hero = heroes.GetModel(heroIndex);
            if (hero.Owner == owner)
            {
                return document;
            }

            if (document.Castles().FindByGarrisonedHero(hero.Id) >= 0)
            {
                throw new GameRuleException($"{hero.Name} is garrisoned in a castle. How the game keeps the owner of a garrisoned hero is not known yet, so it is not changed: move the hero out of the castle in the game first.");
            }

            var players = document.Players();
            var owners = document.HeroOwners();
            var table = owners.Count > 0 ? owners.GetModel(0) : null;
            var edits = new List<(int Index, PlayerModel Model)>();
            if (!owner.IsNeutral)
            {
                ThrowIfDrawnByTheGame(hero, players, table);
                var gaining = players.FindByColor(owner);
                if (gaining >= 0)
                {
                    edits.Add((gaining, players.GetModel(gaining).WithHero(hero.Id)));
                }
            }

            var losing = hero.Owner.IsNeutral ? -1 : players.FindByColor(hero.Owner);
            if (losing >= 0)
            {
                edits.Add((losing, players.GetModel(losing).WithoutHero(hero.Id)));
            }

            var updated = document.WithHero(heroIndex, heroes.Serializer.Serialize(heroes[heroIndex], hero.WithOwner(owner)));
            updated = WithPlayers(updated, edits);
            return table != null && table.Has(hero.Id)
                ? updated.WithHeroOwners(owners.Serializer.Serialize(owners[0], table.WithOwner(hero.Id, owner)))
                : updated;
        }

        /// <summary>
        /// Returns the document with the owner of the hero as it is in <paramref name="original"/>, in the hero, in the lists of heroes of the
        /// players (<see cref="PlayerModel.WithHeroAsIn"/>: the hero goes back to its place and the selected hero comes back) and in the table of
        /// hero owners. The rest of the hero, such as the place it was taken off the map from, is left as it is: it is a change of its own.
        /// </summary>
        /// <param name="document">The document as it is.</param>
        /// <param name="original">The document as the savefile had it, with the same records.</param>
        /// <param name="heroIndex">The place of the hero among the heroes.</param>
        /// <exception cref="GameRuleException">The hero goes back to a list of heroes that has become full.</exception>
        public static SaveDocument WithOwnerAsIn(SaveDocument document, SaveDocument original, int heroIndex)
        {
            var heroes = document.Heroes();
            var hero = heroes.GetModel(heroIndex);
            var originalHero = original.Heroes().GetModel(heroIndex);

            var players = document.Players();
            var originalPlayers = original.Players();
            var edits = new List<(int Index, PlayerModel Model)>();
            for (var index = 0; index < players.Count && index < originalPlayers.Count; index++)
            {
                var player = players.GetModel(index);
                var originalPlayer = originalPlayers.GetModel(index);
                if (player.HeroIds.Contains(hero.Id) || originalPlayer.HeroIds.Contains(hero.Id))
                {
                    edits.Add((index, player.WithHeroAsIn(hero.Id, originalPlayer)));
                }
            }

            var updated = hero.Owner == originalHero.Owner
                ? document
                : document.WithHero(heroIndex, heroes.Serializer.Serialize(heroes[heroIndex], hero with { Owner = originalHero.Owner }));
            updated = WithPlayers(updated, edits);

            var owners = document.HeroOwners();
            var originalOwners = original.HeroOwners();
            if (owners.Count == 0 || originalOwners.Count == 0)
            {
                return updated;
            }

            var table = owners.GetModel(0);
            var originalTable = originalOwners.GetModel(0);
            return table.Has(hero.Id) && originalTable.Has(hero.Id) && table.Heroes[hero.Id] != originalTable.Heroes[hero.Id]
                ? updated.WithHeroOwners(owners.Serializer.Serialize(owners[0], table.With(hero.Id, originalTable.Heroes[hero.Id])))
                : updated;
        }

        // A hero a tavern offers is replaced there by a hero the game draws when it is hired, which the editor cannot do; a hero the game keeps
        // out of play (the campaign heroes, the 41 of hero 3) is kept out for a reason that is not known.
        private static void ThrowIfDrawnByTheGame(HeroModel hero, PlayerChunkCollection players, HeroOwnersModel? table)
        {
            var tavern = Enumerable.Range(0, players.Count).Select(players.GetModel).FirstOrDefault(p => p.Tavern.Ids.Contains(hero.Id));
            if (tavern != null)
            {
                throw new GameRuleException($"{hero.Name} is offered in the tavern of {tavern.Color}. When a hero is hired the game puts another one in its place in the tavern, which the editor cannot do yet: hire the hero in the game, or give it to a player once the tavern offers another.");
            }

            if (table != null && table.Has(hero.Id) && table.Heroes[hero.Id].IsReserved)
            {
                throw new GameRuleException($"{hero.Name} is kept out of play by the game, for a reason the editor does not know yet, so it is not given to a player.");
            }
        }

        private static SaveDocument WithPlayers(SaveDocument document, IEnumerable<(int Index, PlayerModel Model)> edits)
        {
            var players = document.Players();
            foreach (var (index, model) in edits)
            {
                if (model != players.GetModel(index))
                {
                    document = document.WithPlayer(index, players.Serializer.Serialize(players[index], model));
                    players = document.Players();
                }
            }

            return document;
        }
    }
}
