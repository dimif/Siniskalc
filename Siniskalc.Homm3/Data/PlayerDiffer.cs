namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Finds the differences between two versions of the same player. Used for the pending changes list.
    /// </summary>
    public sealed class PlayerDiffer : IPlayerDiffer
    {
        private readonly IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges;

        /// <param name="pendingChanges">Makes the pending changes of what the differ finds.</param>
        public PlayerDiffer(IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges)
        {
            this.pendingChanges = pendingChanges;
        }

        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int playerIndex, PlayerModel original, PlayerModel current, Func<int, string> heroName)
        {
            var changes = new ChangeSink<PlayerModel>((change, revert) => this.pendingChanges.ForPlayer(current.Color.ToString(), playerIndex, change, revert));
            changes.AddResources(original.Resources, current.Resources, p => p.Resources, (p, resources) => p with { Resources = resources });
            changes.AddTavern(original.Tavern, current.Tavern, heroName, p => p.Tavern, (p, tavern) => p with { Tavern = tavern });
            changes.AddDaysLeft(original.DaysLeft, current.DaysLeft, (p, days) => p with { DaysLeft = days });

            // The list changes with the owners of heroes, so putting it back gives those heroes their owners back (PlayerEditSession.RestoreHeroes).
            var heroes = new ChangeSink<PlayerModel>((change, _) => new PendingChange(current.Color.ToString(), change, Homm3ChunkKinds.Player, playerIndex, history => history.CreatePlayerSession(playerIndex).RestoreHeroes()));
            if (!original.HeroIds.Equals(current.HeroIds) || original.SelectedHero != current.SelectedHero)
            {
                heroes.Add(ChangeAreas.Heroes, "Heroes", HeroList(original, heroName), HeroList(current, heroName), p => p);
            }

            return changes.All.Concat(heroes.All).ToList();
        }

        private static string HeroList(PlayerModel player, Func<int, string> heroName)
        {
            var list = player.HeroIds.Count == 0 ? "none" : string.Join(", ", player.HeroIds.Select(heroName));
            return player.SelectedHero is { } selected ? $"{list} ({heroName(selected)} selected)" : list;
        }
    }
}
