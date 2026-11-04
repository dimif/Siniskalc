namespace Siniskalc.Homm3.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>A creature bank's retained rewards, preset and defeated state.</summary>
    public sealed record CreatureBankModel(string Name, MapPosition Position, CreatureBankKind Kind, int Strength, bool IsCleared, bool UpgradedCenter, IReadOnlyList<Artifact?> Artifacts)
    {
        public CreatureBankModel WithArtifact(int slot, Artifact? artifact)
        {
            RangeGuard.ThrowIfNotIndex(slot, this.Artifacts.Count, nameof(slot));
            var artifacts = this.Artifacts.ToArray();
            artifacts[slot] = artifact;
            return this with { Artifacts = artifacts };
        }

        public bool Equals(CreatureBankModel? other) => other != null
            && this.Name == other.Name && this.Position == other.Position
            && this.Kind == other.Kind && this.Strength == other.Strength && this.IsCleared == other.IsCleared
            && this.UpgradedCenter == other.UpgradedCenter
            && this.Artifacts.SequenceEqual(other.Artifacts);

        public override int GetHashCode() => HashCode.Combine(this.Name, this.Position, this.Kind, this.Strength, this.IsCleared, this.UpgradedCenter);
    }
}
