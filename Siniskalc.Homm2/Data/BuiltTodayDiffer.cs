namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists the castles that were marked or unmarked as having built today.
    /// </summary>
    public sealed class BuiltTodayDiffer : IDiffer<BuiltTodayModel>
    {
        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int index, BuiltTodayModel original, BuiltTodayModel current)
        {
            var changes = new ChangeSink<BuiltTodayModel>((change, revert) => ChunkPendingChange.For<MapObjectChunk, BuiltTodayModel>(Homm2ChunkKinds.BuiltToday, current.Name, index, change, revert));
            foreach (var castle in original.Castles.Concat(current.Castles).Distinct().Order().Where(castle => original.Has(castle) != current.Has(castle)))
            {
                changes.Add(ChangeAreas.General, $"Built this turn (castle {castle})", original.Has(castle) ? "Yes" : "No", current.Has(castle) ? "Yes" : "No", set => set.With(castle, original.Has(castle)));
            }

            return changes.All;
        }
    }
}
