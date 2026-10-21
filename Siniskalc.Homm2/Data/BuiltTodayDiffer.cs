namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists the castles that were marked or unmarked as having built today.
    /// </summary>
    public sealed class BuiltTodayDiffer : IMapObjectDiffer<BuiltTodayModel>
    {
        private readonly IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges;

        /// <param name="pendingChanges">Makes the pending changes of what the differ finds.</param>
        public BuiltTodayDiffer(IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges)
        {
            this.pendingChanges = pendingChanges;
        }

        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int index, BuiltTodayModel original, BuiltTodayModel current)
        {
            return original.Castles.Concat(current.Castles).Distinct().Order()
                .Where(castle => original.Has(castle) != current.Has(castle))
                .Select(castle => this.pendingChanges.ForMapObject<BuiltTodayModel>(
                    Homm2ChunkKinds.BuiltToday,
                    current.Name,
                    index,
                    new Change(ChangeAreas.General, $"Built this turn (castle {castle})", original.Has(castle) ? "Yes" : "No", current.Has(castle) ? "Yes" : "No"),
                    set => set.With(castle, original.Has(castle))))
                .ToList();
        }
    }
}
