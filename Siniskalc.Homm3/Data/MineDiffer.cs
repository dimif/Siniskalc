namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Lists the changes of the owner and of the guard of a mine.
    /// </summary>
    public sealed class MineDiffer : IDiffer<MineModel>
    {
        /// <summary>
        /// Lists what an edit changed in a mine.
        /// </summary>
        /// <param name="index">The place in its list, from 0.</param>
        /// <param name="original">The model as it was read from the savefile.</param>
        /// <param name="current">The model after the edit.</param>
        /// <returns>The changes, none when the two are the same.</returns>
        public IReadOnlyList<PendingChange> Diff(int index, MineModel original, MineModel current)
        {
            var changes = new ChangeSink<MineModel>((change, revert) => ChunkPendingChange.For<RecordChunk, MineModel>(Homm3ChunkKinds.Mines, current.Name, index, change, revert));
            changes.AddOwner(original.Owner, current.Owner, (mine, owner) => mine with { Owner = owner });
            changes.AddArmy(ChangeAreas.Garrison, original.Guard, current.Guard, mine => mine.Guard, (mine, army) => mine with { Guard = army });
            return changes.All;
        }
    }
}
