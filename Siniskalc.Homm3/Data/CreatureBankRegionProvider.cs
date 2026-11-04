namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;

    /// <summary>Describes the bank record, not its separate map entrance or occupying hero.</summary>
    public sealed class CreatureBankRegionProvider
    {
        public IReadOnlyList<ByteRegion> GetRegions(CreatureBankChunk chunk) =>
            new List<ByteRegion>
            {
                Slots("Guard creatures", 0, Constants.CreatureSlotCount, ByteRegionKind.Creatures),
                Slots("Guard counts", CreatureBankLayout.Counts, Constants.CreatureSlotCount, ByteRegionKind.Creatures),
                Slots("Resources", CreatureBankLayout.Resources, Constants.CreatureBank.ResourceCount, ByteRegionKind.Resources),
                new("Reward creature", CreatureBankLayout.RewardCreature, 4, ByteRegionKind.Creatures),
                new("Reward creature count", CreatureBankLayout.RewardCreatureCount, 1, ByteRegionKind.Creatures),
                new("Artifact count", CreatureBankLayout.ArtifactCount, 2, ByteRegionKind.Artifacts),
                Slots("Artifacts", CreatureBankLayout.Artifacts, Constants.CreatureBank.ArtifactSlots, ByteRegionKind.Artifacts),
            }.Where(r => r.End <= chunk.Length).ToList();

        private static ByteRegion Slots(string name, int start, int count, ByteRegionKind kind) =>
            new(name, start, count * 4, kind, null, null, Enumerable.Range(0, count).Select(i => new ByteRegion($"{name} {i + 1}", start + (i * 4), 4, kind, name)).ToList());
    }
}
