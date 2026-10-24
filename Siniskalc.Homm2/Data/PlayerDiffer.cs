namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what an edit changed in a player. What the game works out itself (the income of the day) is not listed, but it is saved.
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
            var name = current.Color.ToString();

            var changes = new ChangeSink<PlayerModel>((change, revert) => this.pendingChanges.ForPlayer(name, playerIndex, change, revert));

            changes.AddResources(original.Resources, current.Resources, p => p.Resources, (p, resources) => p with { Resources = resources });
            changes.AddDaysLeft(original.DaysLeft, current.DaysLeft, (p, days) => p with { DaysLeft = days });
            changes.AddTavern(original.Tavern, current.Tavern, heroName, p => p.Tavern, (p, tavern) => p with { Tavern = tavern });

            // The lists change with the owners of heroes and castles, so putting one back gives those heroes or castles their owners back
            // (PlayerEditSession.RestoreHeroes, RestoreCastles), in one step.
            var head = changes.All.Count;
            var lists = new List<PendingChange>();
            if (!original.HeroIds.Equals(current.HeroIds))
            {
                lists.Add(new PendingChange(name, new Change(ChangeAreas.Heroes, "Heroes", List(original.HeroIds), List(current.HeroIds)), Homm2ChunkKinds.Player, playerIndex, history => history.CreatePlayerSession(playerIndex).RestoreHeroes()));
            }

            if (!original.CastleIds.Equals(current.CastleIds))
            {
                lists.Add(new PendingChange(name, new Change(ChangeAreas.Castles, "Castles", List(original.CastleIds), List(current.CastleIds)), Homm2ChunkKinds.Player, playerIndex, history => history.CreatePlayerSession(playerIndex).RestoreCastles()));
            }

            if (original.SelectedHero != current.SelectedHero)
            {
                changes.Add(ChangeAreas.General, "Selected hero", ChangeText.Number(original.SelectedHero), ChangeText.Number(current.SelectedHero), p => p with { SelectedHero = original.SelectedHero });
            }

            if (original.SelectedCastle != current.SelectedCastle)
            {
                changes.Add(ChangeAreas.General, "Selected castle", ChangeText.Number(original.SelectedCastle), ChangeText.Number(current.SelectedCastle), p => p with { SelectedCastle = original.SelectedCastle });
            }

            return changes.All.Take(head).Concat(lists).Concat(changes.All.Skip(head)).ToList();
        }

        private static string List(Numbers ids) => ids.Count == 0 ? "none" : string.Join(", ", ids);
    }
}
