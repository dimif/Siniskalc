namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Lists the change of whose turn it is.
    /// </summary>
    public sealed class TurnDiffer : IDiffer<TurnModel>
    {
        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int index, TurnModel original, TurnModel current)
        {
            var changes = new ChangeSink<TurnModel>((change, revert) => ChunkPendingChange.For<RecordChunk, TurnModel>(Homm3ChunkKinds.Turn, current.Name, index, change, revert));
            if (original.Color != current.Color)
            {
                changes.Add(ChangeAreas.General, "Player", original.Color.ToString(), current.Color.ToString(), turn => turn with { Color = original.Color });
            }

            return changes.All;
        }
    }
}
