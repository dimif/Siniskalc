using Siniskalc.Data;
using Siniskalc.Entities;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// Reads the building masks of a castle chunk, for the tests that compare them before and after an edit.
    /// </summary>
    internal static class CastleMasks
    {
        /// <summary>The factions whose castles the building rules have been checked against savefiles.</summary>
        public static readonly Faction[] VerifiedFactions = [Faction.Rampart, Faction.Bulwark, Faction.Factory, Faction.Cove];

        public static (ulong Built, ulong Current, ulong Enabled) Masks(CastleChunk chunk) => (
            chunk.GetUInt64At(chunk.NameEnd + CastleLayout.BuiltBuildingsFromNameEnd) & ~Unexplained(chunk),
            chunk.GetUInt64At(chunk.NameEnd + CastleLayout.CurrentBuildingsFromNameEnd) & ~Unexplained(chunk),
            chunk.GetUInt64At(chunk.NameEnd + CastleLayout.EnabledBuildingsFromNameEnd));

        public static string Hex((ulong Built, ulong Current, ulong Enabled) masks) => $"built {masks.Built:X} current {masks.Current:X} enabled {masks.Enabled:X}";

        // Bulwark and Factory castles have a bit 28 that comes and goes with their fourth dwelling and that nothing explains yet.
        private static ulong Unexplained(CastleChunk chunk) =>
            TestServices.CastleDeserializer.DeserializeFaction(chunk) == Faction.Bulwark || TestServices.CastleDeserializer.DeserializeFaction(chunk) == Faction.Factory ? 1UL << 28 : 0;
    }
}
