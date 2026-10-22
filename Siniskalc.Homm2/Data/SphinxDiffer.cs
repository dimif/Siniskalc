namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Lists what changed in a sphinx: whether it is active, the reward, the answers and the riddle.
    /// </summary>
    public sealed class SphinxDiffer : IMapObjectDiffer<SphinxModel>
    {
        private static readonly string[] Resources = ["Wood", "Mercury", "Ore", "Sulfur", "Crystal", "Gems", "Gold"];

        /// <inheritdoc/>
        public IReadOnlyList<PendingChange> Diff(int index, SphinxModel original, SphinxModel current)
        {
            var changes = new ChangeSink<SphinxModel>((change, revert) => ChunkPendingChange.For<MapObjectChunk, SphinxModel>(Homm2ChunkKinds.Sphinxes, current.Name, index, change, revert));

            if (original.IsActive != current.IsActive)
            {
                changes.Add(ChangeAreas.General, "Riddle", original.IsActive ? "not answered" : "answered", current.IsActive ? "not answered" : "answered", s => s with { IsActive = original.IsActive });
            }

            for (var i = 0; i < Resources.Length; i++)
            {
                var resource = i;
                if (original.Reward[i] != current.Reward[i])
                {
                    changes.Add(ChangeAreas.General, $"Reward: {Resources[i]}", original.Reward[i].ToString("N0"), current.Reward[i].ToString("N0"), s => s with { Reward = s.Reward.Select((v, j) => j == resource ? original.Reward[resource] : v).ToList() });
                }
            }

            if (original.ArtifactCode != current.ArtifactCode)
            {
                changes.Add(ChangeAreas.General, "Reward: artifact", original.ArtifactCode.ToString(), current.ArtifactCode.ToString(), s => s with { ArtifactCode = original.ArtifactCode });
            }

            if (!original.Answers.SequenceEqual(current.Answers))
            {
                changes.Add(ChangeAreas.General, "Answers", string.Join(", ", original.Answers), string.Join(", ", current.Answers), s => s with { Answers = original.Answers });
            }

            if (original.Riddle != current.Riddle)
            {
                changes.Add(ChangeAreas.General, "Riddle text", original.Riddle, current.Riddle, s => s with { Riddle = original.Riddle });
            }

            return changes.All;
        }
    }
}
