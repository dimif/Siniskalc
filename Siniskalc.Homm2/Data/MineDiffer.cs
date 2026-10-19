namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists the change of the owner of a mine.
    /// </summary>
    public sealed class MineDiffer : IMapObjectDiffer<MineModel>
    {
        private readonly IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits> pendingChanges;

        /// <param name="pendingChanges">Makes the pending changes of what the differ finds.</param>
        public MineDiffer(IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits> pendingChanges)
        {
            this.pendingChanges = pendingChanges;
        }

        public IReadOnlyList<PendingChange> Diff(int index, MineModel original, MineModel current)
        {
            var changes = new List<PendingChange>();
            if (original.Owner != current.Owner)
            {
                changes.Add(this.pendingChanges.ForMapObject<MineModel>(
                    Homm2ChunkKinds.Mines,
                    current.Name,
                    index,
                    new Change(ChangeAreas.General, "Owner", OwnerNames<Siniskalc.Homm2.Entities.Color>.Of(original.Owner), OwnerNames<Siniskalc.Homm2.Entities.Color>.Of(current.Owner)),
                    mine => mine with { Owner = original.Owner }));
            }

            return changes;
        }
    }
}
