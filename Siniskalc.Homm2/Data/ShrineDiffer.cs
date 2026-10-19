namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists the change of the spell of a shrine.
    /// </summary>
    public sealed class ShrineDiffer : IMapObjectDiffer<ShrineModel>
    {
        private readonly IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits> pendingChanges;

        /// <param name="pendingChanges">Makes the pending changes of what the differ finds.</param>
        public ShrineDiffer(IPendingChangesFactory<HeroModel, PlayerModel, CastleModel, ObeliskVisits> pendingChanges)
        {
            this.pendingChanges = pendingChanges;
        }

        public IReadOnlyList<PendingChange> Diff(int index, ShrineModel original, ShrineModel current)
        {
            var changes = new List<PendingChange>();
            if (original.SpellCode != current.SpellCode)
            {
                changes.Add(this.pendingChanges.ForMapObject<ShrineModel>(
                    Homm2ChunkKinds.Shrines,
                    current.Name,
                    index,
                    new Change(ChangeAreas.General, "Spell", SpellText(original), SpellText(current)),
                    shrine => shrine with { SpellCode = original.SpellCode }));
            }

            return changes;
        }

        private static string SpellText(ShrineModel shrine) => shrine.Spell?.Name ?? $"Spell {shrine.SpellCode}";
    }
}
