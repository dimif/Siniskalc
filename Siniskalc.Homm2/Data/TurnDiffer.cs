namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists the change of whose turn it is.
    /// </summary>
    public sealed class TurnDiffer : IMapObjectDiffer<TurnModel>
    {
        private readonly IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges;

        /// <param name="pendingChanges">Makes the pending changes of what the differ finds.</param>
        public TurnDiffer(IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits<PlayerColor>> pendingChanges)
        {
            this.pendingChanges = pendingChanges;
        }

        public IReadOnlyList<PendingChange> Diff(int index, TurnModel original, TurnModel current)
        {
            var changes = new List<PendingChange>();
            if (original.Record != current.Record)
            {
                changes.Add(this.pendingChanges.ForMapObject<TurnModel>(
                    Homm2ChunkKinds.Turn,
                    current.Name,
                    index,
                    new Change(ChangeAreas.General, "Player", original.Color?.ToString() ?? $"record {original.Record}", current.Color?.ToString() ?? $"record {current.Record}"),
                    turn => turn with { Record = original.Record, Color = original.Color }));
            }

            return changes;
        }
    }
}
