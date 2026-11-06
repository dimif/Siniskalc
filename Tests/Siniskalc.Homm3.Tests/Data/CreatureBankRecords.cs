using Siniskalc.Core.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Records and models of creature banks made by hand, for the tests of the reader, writer, differ and reconciler of a bank.
    /// </summary>
    internal static class CreatureBankRecords
    {
        public const int Size = 128;

        public static readonly MapPosition Position = new(5, 6);

        public static EntityCatalog Catalog => TestCatalog.For(GameVersion.HornOfTheAbyss);

        /// <summary>A code that is no wearable artifact of the catalog, and that is not the byte FF.</summary>
        public static uint UnknownArtifactCode => (uint)Enumerable.Range(0, 255).First(c => Catalog.Artifacts.Find((byte)c) is not { Wearable: true });

        public static CreatureBankChunk Chunk(CreatureBankKind kind, int strength = 1, bool cleared = false, bool upgraded = false, uint[]? artifacts = null, uint entrance = 0x1F, int? occupyingHero = null)
        {
            var bytes = new byte[Size];
            var types = cleared ? Enumerable.Repeat(-1, Constants.CreatureSlotCount).ToArray() : CreatureBankPresets.Types(kind, upgraded);
            var counts = cleared ? new int[Constants.CreatureSlotCount] : CreatureBankPresets.Counts(kind, strength);
            var resources = CreatureBankPresets.Resources(kind, strength);
            for (var slot = 0; slot < Constants.CreatureSlotCount; slot++)
            {
                BitConverter.GetBytes(types[slot]).CopyTo(bytes, slot * 4);
                BitConverter.GetBytes(counts[slot]).CopyTo(bytes, CreatureBankLayout.Counts + (slot * 4));
                BitConverter.GetBytes(resources[slot]).CopyTo(bytes, CreatureBankLayout.Resources + (slot * 4));
            }

            var codes = artifacts ?? (kind == CreatureBankKind.DragonUtopia ? Enumerable.Repeat(uint.MaxValue, Constants.CreatureBank.ArtifactSlots).ToArray() : []);
            for (var slot = 0; slot < codes.Length; slot++)
            {
                BitConverter.GetBytes(codes[slot]).CopyTo(bytes, CreatureBankLayout.Artifacts + (slot * 4));
            }

            return new CreatureBankChunk($"{kind} 3", bytes, 1000, 3, 3, Position, kind, new RawBytesChunk("Entrance", BitConverter.GetBytes(entrance), 5000), occupyingHero);
        }

        public static CreatureBankChunk WithBytes(CreatureBankChunk chunk, params (int Offset, byte Value)[] bytes)
        {
            var copy = chunk.ToArray();
            foreach (var (offset, value) in bytes)
            {
                copy[offset] = value;
            }

            return chunk.WithBytes(copy);
        }

        public static CreatureBankModel Model(CreatureBankKind kind, int strength = 1, bool cleared = false, bool upgraded = false, Artifact?[]? artifacts = null) =>
            new($"{kind} 3", Position, kind, strength, cleared, upgraded, artifacts ?? (kind == CreatureBankKind.DragonUtopia ? new Artifact?[Constants.CreatureBank.ArtifactSlots] : []));
    }
}
