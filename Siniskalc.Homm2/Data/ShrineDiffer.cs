namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists the change of the spell of a shrine.
    /// </summary>
    public sealed class ShrineDiffer : IMapObjectDiffer<ShrineModel>
    {
        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int index, ShrineModel original, ShrineModel current)
        {
            var changes = new ChangeSink<ShrineModel>((change, revert) => ChunkPendingChange.For<MapObjectChunk, ShrineModel>(Homm2ChunkKinds.Shrines, current.Name, index, change, revert));
            if (original.SpellCode != current.SpellCode)
            {
                changes.Add(ChangeAreas.General, "Spell", SpellText(original), SpellText(current), shrine => shrine with { SpellCode = original.SpellCode });
            }

            return changes.All;
        }

        private static string SpellText(ShrineModel shrine) => shrine.Spell?.Name ?? $"Spell {shrine.SpellCode}";
    }
}
