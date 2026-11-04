namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Globalization;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Models;

    public sealed class CreatureBankDiffer : IDiffer<CreatureBankModel>
    {
        public IReadOnlyList<PendingChange> Diff(int index, CreatureBankModel original, CreatureBankModel current)
        {
            var changes = new ChangeSink<CreatureBankModel>((change, revert) =>
                ChunkPendingChange.For<CreatureBankChunk, CreatureBankModel>(Homm3ChunkKinds.CreatureBanks, current.Name, index, change, revert));
            if (original.IsCleared != current.IsCleared)
            {
                changes.Add(ChangeAreas.General, "Cleared", original.IsCleared ? "Yes" : "No", current.IsCleared ? "Yes" : "No", m => m with { IsCleared = original.IsCleared, UpgradedCenter = original.UpgradedCenter });
            }

            if (original.Strength != current.Strength)
            {
                changes.Add(ChangeAreas.General, "Guard strength and rewards", original.Strength.ToString(CultureInfo.InvariantCulture), current.Strength.ToString(CultureInfo.InvariantCulture), m => m with { Strength = original.Strength });
            }

            if (original.UpgradedCenter != current.UpgradedCenter && !current.IsCleared)
            {
                changes.Add(ChangeAreas.General, "Upgraded centre", original.UpgradedCenter ? "Yes" : "No", current.UpgradedCenter ? "Yes" : "No", m => m with { UpgradedCenter = original.UpgradedCenter });
            }

            for (var slot = 0; slot < current.Artifacts.Count; slot++)
            {
                var number = slot;
                if (original.Artifacts[slot] != current.Artifacts[slot])
                {
                    changes.Add(ChangeAreas.General, $"Artifact {slot + 1}", original.Artifacts[slot]?.Name ?? "(none)", current.Artifacts[slot]?.Name ?? "(none)", m => m.WithArtifact(number, original.Artifacts[number]));
                }
            }

            return changes.All;
        }
    }
}
