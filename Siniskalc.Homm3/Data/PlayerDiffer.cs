namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Homm3.Entities;
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
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
            return changes.All;
        }
    }
}
