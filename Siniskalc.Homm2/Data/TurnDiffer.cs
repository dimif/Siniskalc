namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists the change of whose turn it is.
    /// </summary>
    public sealed class TurnDiffer : IDiffer<TurnModel>
    {
        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int index, TurnModel original, TurnModel current)
        {
            var changes = new ChangeSink<TurnModel>((change, revert) => ChunkPendingChange.For<MapObjectChunk, TurnModel>(Homm2ChunkKinds.Turn, current.Name, index, change, revert));
            if (original.Record != current.Record)
            {
                changes.Add(
                    ChangeAreas.General,
                    "Player",
                    original.Color?.ToString() ?? $"record {original.Record}",
                    current.Color?.ToString() ?? $"record {current.Record}",
                    turn => turn with { Record = original.Record, Color = original.Color });
            }

            return changes.All;
        }
    }
}
