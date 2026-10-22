namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Lists the changes of the owner and of the guard of a mine.
    /// </summary>
    public sealed class MineDiffer : IMineDiffer
    {
        public IReadOnlyList<PendingChange> Diff(int index, MineModel original, MineModel current)
        {
            var changes = new ChangeSink<MineModel>((change, revert) => ChunkPendingChange.For<MineChunk, MineModel>(Homm3ChunkKinds.Mines, current.Name, index, change, revert));
            changes.AddOwner(original.Owner, current.Owner, (mine, owner) => mine with { Owner = owner });
            changes.AddArmy(ChangeAreas.Garrison, original.Guard, current.Guard, mine => mine.Guard, (mine, army) => mine with { Guard = army });
            return changes.All;
        }
    }
}
