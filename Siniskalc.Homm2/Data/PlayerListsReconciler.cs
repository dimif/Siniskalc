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
    /// Puts the lists of heroes and castles of the players in step with the owners of the heroes and the castles after an edit, as the game
    /// keeps them: a hero or a castle that changed hands leaves the list of every other player and joins the list of its owner
    /// (<see cref="PlayerLists.WithHero"/>, <see cref="PlayerLists.WithCastle"/>), and a player who loses its last castle starts counting the
    /// days to own one again while a player who gains one stops (<see cref="PlayerModel.WithTownLost"/>, <see cref="PlayerModel.WithTownGained"/>).
    /// A list that is back as the savegame had it gets its selection and its days left back too, so putting an owner back puts back what
    /// followed it. Only the players whose lists change are written.
    /// </summary>
    public sealed class PlayerListsReconciler : IDocumentReconciler
    {
        private static readonly ILogger<PlayerListsReconciler> Logger = LogFactory.For<PlayerListsReconciler>();

        /// <inheritdoc/>
        /// <exception cref="GameRuleException">A hero or a castle joins a list that is full; nothing is written.</exception>
        public SaveDocument Reconcile(SaveDocument original, SaveDocument before, SaveDocument after)
        {
            var heroMoves = HeroMoves(before.Heroes(), after.Heroes());
            var castleMoves = CastleMoves(before.Castles(), after.Castles());
            if (heroMoves.Count == 0 && castleMoves.Count == 0)
            {
                return after;
            }

            var players = after.Players();
            var originalPlayers = original.Players();
            var lists = new Dictionary<int, PlayerLists>();
            var models = new Dictionary<int, PlayerModel>();

            // Every new record is made before any is written, so a full list leaves the savegame as it was.
            for (var index = 0; index < players.Count && index < originalPlayers.Count; index++)
            {
                var loaded = originalPlayers.GetLists(index);
                var current = players.GetLists(index);
                var next = current;
                foreach (var (heroId, owner) in heroMoves)
                {
                    next = next.WithHero(heroId, Owns(players, owner, index), loaded);
                }

                var player = players.GetModel(index);
                var model = player;
                foreach (var (castleIndex, owner) in castleMoves)
                {
                    var had = next.CastleIds.Contains(castleIndex);
                    next = next.WithCastle(castleIndex, Owns(players, owner, index), loaded);
                    var has = next.CastleIds.Contains(castleIndex);
                    if (had != has)
                    {
                        model = next.CastleIds.Equals(loaded.CastleIds)
                            ? model with { DaysLeft = originalPlayers.GetModel(index).DaysLeft }
                            : has ? model.WithTownGained() : model.WithTownLost(next.CastleIds.Count);
                    }
                }

                if (next != current)
                {
                    lists[index] = next;
                }

                if (model != player)
                {
                    models[index] = model;
                }
            }

            var updated = after;
            for (var index = 0; index < players.Count; index++)
            {
                var chunk = players[index];
                var written = lists.TryGetValue(index, out var playerLists) ? players.ListsSerializer.Serialize(chunk, playerLists) : chunk;
                written = models.TryGetValue(index, out var model) ? players.Serializer.Serialize(written, model) : written;
                if (!ReferenceEquals(written, chunk))
                {
                    Logger.LogDebug("Putting the lists of {Player} in step with the owners", players.GetName(index));
                    updated = updated.WithChunk(Homm2ChunkKinds.Player, index, written);
                }
            }

            return updated;
        }

        private static bool Owns(PlayerChunkCollection players, PlayerColor owner, int index) => !owner.IsNeutral && players.FindByColor(owner) == index;

        // The heroes whose owner the edit changed, with their new owner. A hero whose record was not touched is not read.
        private static List<(int HeroId, PlayerColor Owner)> HeroMoves(HeroChunkCollection before, HeroChunkCollection after)
        {
            var moves = new List<(int, PlayerColor)>();
            for (var index = 0; index < before.Count && index < after.Count; index++)
            {
                if (ReferenceEquals(before[index], after[index]))
                {
                    continue;
                }

                var (was, now) = (before.GetModel(index), after.GetModel(index));
                if (was.Owner != now.Owner)
                {
                    moves.Add((now.Id, now.Owner));
                }
            }

            return moves;
        }

        // The castles whose owner the edit changed, with their new owner. The number of a castle is its place among the castles.
        private static List<(int CastleIndex, PlayerColor Owner)> CastleMoves(CastleChunkCollection before, CastleChunkCollection after)
        {
            var moves = new List<(int, PlayerColor)>();
            for (var index = 0; index < before.Count && index < after.Count; index++)
            {
                if (ReferenceEquals(before[index], after[index]))
                {
                    continue;
                }

                var (was, now) = (before.GetModel(index).Owner, after.GetModel(index).Owner);
                if (was != now)
                {
                    moves.Add((index, now));
                }
            }

            return moves;
        }
    }
}
