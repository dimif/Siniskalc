using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Reads the building masks of a castle chunk, for the tests that compare them before and after an edit.
    /// </summary>
    internal static class CastleMasks
    {
        /// <summary>The factions whose castles the building rules have been checked against savefiles.</summary>
        public static readonly Faction[] VerifiedFactions = [TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Bulwark"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Factory"), TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Cove")];

        public static (ulong Built, ulong Current, ulong Enabled) Masks(CastleChunk chunk) => (
            chunk.GetUInt64At(chunk.NameEnd + CastleLayout.BuiltBuildingsFromNameEnd) & ~Unexplained(chunk),
            chunk.GetUInt64At(chunk.NameEnd + CastleLayout.VisibleBuildingsFromNameEnd) & ~Unexplained(chunk),
            chunk.GetUInt64At(chunk.NameEnd + CastleLayout.EnabledBuildingsFromNameEnd));

        public static string Hex((ulong Built, ulong Current, ulong Enabled) masks) => $"built {masks.Built:X} current {masks.Current:X} enabled {masks.Enabled:X}";

        // Bulwark and Factory castles have a bit 28 that comes and goes with their fourth dwelling and that nothing explains yet.
        private static ulong Unexplained(CastleChunk chunk) =>
            TestServices.CastleDeserializer.DeserializeFaction(chunk) == TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Bulwark") || TestServices.CastleDeserializer.DeserializeFaction(chunk) == TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Factory") ? 1UL << 28 : 0;
    }
}
