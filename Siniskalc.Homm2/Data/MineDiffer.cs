namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists the change of the owner of a mine.
    /// </summary>
    public sealed class MineDiffer : IDiffer<MineModel>
    {
        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int index, MineModel original, MineModel current)
        {
            var changes = new ChangeSink<MineModel>((change, revert) => ChunkPendingChange.For<MapObjectChunk, MineModel>(Homm2ChunkKinds.Mines, current.Name, index, change, revert));
            changes.AddOwner(original.Owner, current.Owner, (mine, owner) => mine with { Owner = owner });
            return changes.All;
        }
    }
}
