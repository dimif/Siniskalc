namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Lists the changes of the owner and of the creatures that can be hired of a dwelling.
    /// </summary>
    public sealed class DwellingDiffer : IDiffer<DwellingModel>
    {
        public IReadOnlyList<PendingChange> Diff(int index, DwellingModel original, DwellingModel current)
        {
            var changes = new ChangeSink<DwellingModel>((change, revert) => ChunkPendingChange.For<RecordChunk, DwellingModel>(Homm3ChunkKinds.Dwellings, current.Name, index, change, revert));
            changes.AddOwner(original.Owner, current.Owner, (dwelling, owner) => dwelling with { Owner = owner });
            if (original.Stock != current.Stock)
            {
                changes.Add(ChangeAreas.General, "Creatures to hire", original.Stock.ToString(), current.Stock.ToString(), dwelling => dwelling with { Stock = original.Stock });
            }

            return changes.All;
        }
    }
}
