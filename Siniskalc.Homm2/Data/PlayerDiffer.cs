namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what an edit changed in a player. What the game works out itself (the income of the day) is not listed, but it is saved. The
    /// lists of heroes and castles follow the owners of the heroes and the castles, whose changes are listed with them.
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
            changes.AddTavern(original.Tavern, current.Tavern, heroName, p => p.Tavern, (p, tavern) => p with { Tavern = tavern });
            changes.AddDaysLeft(original.DaysLeft, current.DaysLeft, (p, days) => p with { DaysLeft = days });

            return changes.All;
        }
    }
}
