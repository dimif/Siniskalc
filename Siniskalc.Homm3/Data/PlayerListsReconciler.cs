namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Puts the records that follow the owners of the heroes and the castles in step after an edit, as the game keeps them
    /// (<c>TestData/Hota/HeroOwners</c>): a hero that changed hands leaves the list of heroes of every other player and joins the list of its
    /// owner (<see cref="PlayerLists.WithHero"/>), and the table of hero owners says its owner (<see cref="HeroOwnersModel.WithOwner"/>); a
    /// player's town count and acquisition-ordered town list follow castle ownership (<c>TestData/Hota/TownOwnership</c>), while the selected
    /// town is left alone. A player who loses its last town starts counting the days to own one again, and a player who gains one stops
    /// (<see cref="PlayerModel.WithTownLost"/>, <see cref="PlayerModel.WithTownGained"/>). A hero that goes back to the owner the savegame had
    /// gets its entry of the table back, a list that is back as the savegame had it gets its selected hero back, and a player who owns the
    /// towns the savegame gave it again gets its days left back, so putting an owner back puts back what followed it. Only the records that
    /// change are written.
    /// </summary>
    public sealed class PlayerListsReconciler : IDocumentReconciler
    {
        private static readonly ILogger<PlayerListsReconciler> Logger = LogFactory.For<PlayerListsReconciler>();

        /// <inheritdoc/>
        /// <exception cref="GameRuleException">
        /// The edit gives a hero to a player in a way the editor cannot make as the game would: the hero is garrisoned in a castle, it is offered
        /// in a tavern or kept out of play by the game, or the list of heroes of its new owner is full. Nothing is written.
        /// </exception>
        public SaveDocument Reconcile(SaveDocument original, SaveDocument before, SaveDocument after)
        {
            var heroMoves = HeroMoves(before.Heroes(), after.Heroes());
            var castleMoves = CastleMoves(before.Castles(), after.Castles());
            if (heroMoves.Count == 0 && castleMoves.Count == 0)
            {
                return after;
            }

            var originalHeroes = original.Heroes();
            foreach (var (heroIndex, hero) in heroMoves)
            {
                if (hero.Owner != originalHeroes.GetModel(heroIndex).Owner)
                {
                    ThrowIfNotGivenAsTheGameWould(after, hero);
                }
            }

            // Every new record is made before any is written, so a refused edit leaves the savegame as it was.
            var updated = WithPlayers(after, before, original, heroMoves, castleMoves, Homm3ChunkKinds.Player);
            updated = WithPlayers(updated, before, original, [], castleMoves, Homm3ChunkKinds.InactivePlayer);
            return WithHeroOwners(updated, original, heroMoves);
        }

        // The heroes whose owner the edit changed, with their place among the heroes. A hero whose record was not touched is not read.
        private static List<(int HeroIndex, HeroModel Hero)> HeroMoves(HeroChunkCollection before, HeroChunkCollection after)
        {
            var moves = new List<(int, HeroModel)>();
            for (var index = 0; index < before.Count && index < after.Count; index++)
            {
                if (ReferenceEquals(before[index], after[index]))
                {
                    continue;
                }

                var now = after.GetModel(index);
                if (before.GetModel(index).Owner != now.Owner)
                {
                    moves.Add((index, now));
                }
            }

            return moves;
        }

        // The places of the castles whose owner the edit changed.
        private static List<int> CastleMoves(CastleChunkCollection before, CastleChunkCollection after)
        {
            var moves = new List<int>();
            for (var index = 0; index < before.Count && index < after.Count; index++)
            {
                if (!ReferenceEquals(before[index], after[index]) && before.GetModel(index).Owner != after.GetModel(index).Owner)
                {
                    moves.Add(index);
                }
            }

            return moves;
        }

        // A hero that is garrisoned is in no list, and how the game keeps its owner is not known; a hero a tavern offers is replaced there by a
        // hero the game draws when it is hired, which the editor cannot do; a hero the game keeps out of play (the campaign heroes, the 41 of
        // hero 3) is kept out: a hero disabled in the map editor has this entry (Adrienne, id 145, in a Horn of the Abyss save), so it is refused.
        private static void ThrowIfNotGivenAsTheGameWould(SaveDocument document, HeroModel hero)
        {
            if (document.Castles().FindByGarrisonedHero(hero.Id) >= 0)
            {
                throw new GameRuleException($"{hero.Name} is garrisoned in a castle. How the game keeps the owner of a garrisoned hero is not known yet, so it is not changed: move the hero out of the castle in the game first.");
            }

            if (hero.Owner.IsNeutral)
            {
                return;
            }

            var players = document.Players();
            var tavern = Enumerable.Range(0, players.Count).Select(players.GetModel).FirstOrDefault(p => p.Tavern.Ids.Contains(hero.Id));
            if (tavern != null)
            {
                throw new GameRuleException($"{hero.Name} is offered in the tavern of {tavern.Color}. When a hero is hired the game puts another one in its place in the tavern, which the editor cannot do yet: hire the hero in the game, or give it to a player once the tavern offers another.");
            }

            var owners = document.HeroOwners();
            if (owners.Count > 0 && owners.GetModel(0) is var table && table.Has(hero.Id) && table.Heroes[hero.Id].IsReserved)
            {
                throw new GameRuleException($"{hero.Name} is kept out of play by the game (a hero disabled in the map editor, or a campaign hero), so it is not given to a player.");
            }
        }

        // The lists of heroes and towns and the days left. A player with no record (Neutral, or a savefile whose players are not known)
        // has nothing to put in step.
        private static SaveDocument WithPlayers(SaveDocument after, SaveDocument before, SaveDocument original, List<(int HeroIndex, HeroModel Hero)> heroMoves, List<int> castleMoves, ChunkKind kind)
        {
            var players = (PlayerChunkCollection)after.GetCollection(kind);
            var originalPlayers = (PlayerChunkCollection)original.GetCollection(kind);
            var owners = after.Castles().GetOwners();
            var formerOwners = before.Castles().GetOwners();
            var originalOwners = original.Castles().GetOwners();
            var edits = new List<(int Index, PlayerLists? Lists, PlayerModel? Model)>();
            for (var index = 0; index < players.Count && index < originalPlayers.Count; index++)
            {
                var color = players.GetColor(index);
                var loaded = originalPlayers.GetLists(index);
                var current = players.GetLists(index);
                var lists = current;
                foreach (var (_, hero) in heroMoves)
                {
                    lists = lists.WithHero(hero.Id, hero.Owner == color, loaded);
                }

                if (lists.HeroIds.Count > Constants.Player.HeroListSize)
                {
                    throw new GameRuleException(FullListNotice.For(color.Name, "heroes", Constants.Player.HeroListSize));
                }

                var player = players.GetModel(index);
                var model = player;
                var gains = castleMoves.Any(c => owners[c] == color);
                if (gains || castleMoves.Any(c => formerOwners[c] == color))
                {
                    foreach (var castle in castleMoves.Where(c => owners[c] == color || formerOwners[c] == color))
                    {
                        lists = lists.WithTown(castle, owners[castle] == color, loaded);
                    }

                    if (lists.TownIds.Count > Constants.Player.TownListSize)
                    {
                        throw new GameRuleException(FullListNotice.For(color.Name, "towns", Constants.Player.TownListSize));
                    }

                    var towns = Towns(owners, color);
                    model = towns.SequenceEqual(Towns(originalOwners, color))
                        ? model with { DaysLeft = originalPlayers.GetModel(index).DaysLeft }
                        : gains ? model.WithTownGained() : model.WithTownLost(towns.Count);
                }

                edits.Add((index, lists == current ? null : lists, model == player ? null : model));
            }

            var updated = after;
            var changed = 0;
            foreach (var (index, lists, model) in edits)
            {
                var chunk = players[index];
                var written = lists != null ? players.ListsSerializer.Serialize(chunk, lists) : chunk;
                written = model != null ? players.Serializer.Serialize(written, model) : written;
                if (!ReferenceEquals(written, chunk))
                {
                    changed++;
                    updated = updated.WithChunk(kind, index, written);
                }
            }

            Logger.LogDebug("Put the heroes, towns and days left of {Players} players in step with the owners", changed);

            return updated;
        }

        // The table of hero owners says the new owner of each hero that moved, or the entry the savegame had when the hero is back with the
        // owner it had. A savefile with no table, and a hero the table has no entry for, are left alone.
        private static SaveDocument WithHeroOwners(SaveDocument document, SaveDocument original, List<(int HeroIndex, HeroModel Hero)> heroMoves)
        {
            var owners = document.HeroOwners();
            if (owners.Count == 0 || heroMoves.Count == 0)
            {
                return document;
            }

            var originalHeroes = original.Heroes();
            var originalTable = original.HeroOwners().Count > 0 ? original.HeroOwners().GetModel(0) : null;
            var current = owners.GetModel(0);
            var table = current;
            foreach (var (heroIndex, hero) in heroMoves.Where(m => current.Has(m.Hero.Id)))
            {
                table = hero.Owner == originalHeroes.GetModel(heroIndex).Owner && originalTable != null && originalTable.Has(hero.Id)
                    ? table.With(hero.Id, originalTable.Heroes[hero.Id])
                    : table.WithOwner(hero.Id, hero.Owner);
            }

            return table.Equals(current) ? document : document.WithHeroOwners(owners.Serializer.Serialize(owners[0], table));
        }

        // The places of the castles a player owns, in their order.
        private static List<int> Towns(IReadOnlyList<PlayerColor> owners, PlayerColor color) =>
            Enumerable.Range(0, owners.Count).Where(c => owners[c] == color).ToList();
    }
}
