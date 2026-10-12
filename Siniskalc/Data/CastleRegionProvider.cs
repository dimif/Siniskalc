namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Describes where each known field lives in a castle chunk, using the offsets of <see cref="CastleLayout"/>.
    /// </summary>
    public sealed class CastleRegionProvider : ICastleRegionProvider
    {
        private const int MaskSize = 8;

        /// <inheritdoc/>
        public IReadOnlyList<ByteRegion> GetRegions(CastleChunk chunk)
        {
            var nameEnd = chunk.NameEnd;
            var regions = new List<ByteRegion>
            {
                Make("Owner", CastleLayout.Owner, 1, ByteRegionKind.Identity, "PlayerColor colour; FF means none"),
                Make("Built this turn", CastleLayout.BuiltThisTurn, 1, ByteRegionKind.Other, "1 when something was built this turn"),
                Make("Faction", CastleLayout.Faction, 1, ByteRegionKind.Identity),
                Make("Pos X", CastleLayout.PositionX, 1, ByteRegionKind.Position),
                Make("Pos Y", CastleLayout.PositionY, 1, ByteRegionKind.Position),
                Make("Pos Lvl", CastleLayout.PositionZ, 1, ByteRegionKind.Position),
                Make("Shipyard X", CastleLayout.ShipyardX, 1, ByteRegionKind.Position, "FF when the castle is not on the coast"),
                Make("Shipyard Y", CastleLayout.ShipyardY, 1, ByteRegionKind.Position, "FF when the castle is not on the coast"),
                Slots("Garrison creatures", CastleLayout.GarrisonTypes, Constants.CreatureSlotCount, 4, ByteRegionKind.Creatures),
                Slots("Garrison count", CastleLayout.GarrisonCounts, Constants.CreatureSlotCount, 4, ByteRegionKind.Creatures),
                Make("Garrisoned hero ID", CastleLayout.GarrisonedHero, 1, ByteRegionKind.Identity, "Hero id; FF when there is none"),
                Make("Visiting hero ID", CastleLayout.VisitingHero, 1, ByteRegionKind.Identity, "Hero id; FF when there is none"),
                Make("Mage guild level", CastleLayout.MageGuildLevel, 1, ByteRegionKind.Spells, "0 when there is no mage guild"),
                Make("Unused, always 0", CastleLayout.NameLength - 1, 1, ByteRegionKind.Other, "The last byte of the four in front of the name length"),
                Make("Name length", CastleLayout.NameLength, 2, ByteRegionKind.Identity, "The number of bytes of the name"),
                Make("Name", CastleLayout.Name, nameEnd - CastleLayout.Name, ByteRegionKind.Identity, "Text"),
                Slots("Creatures available", nameEnd + CastleLayout.CreaturesAvailableFromNameEnd, CastleLayout.CreatureLevelCount, 2, ByteRegionKind.Creatures, "Level", "The creatures waiting in the dwelling of each level"),
                Slots("Upgraded creatures available", nameEnd + CastleLayout.UpgradedCreaturesAvailableFromNameEnd, CastleLayout.CreatureLevelCount, 2, ByteRegionKind.Creatures, "Level", "The count moves here when the dwelling is upgraded"),
                Slots("Mage guild spell counts", nameEnd + CastleLayout.SpellCountsFromNameEnd, CastleLayout.SpellLevelCount, 1, ByteRegionKind.Spells, "Level", "The number of spells in the Mage Guild for each of the five spell levels"),
                Make("Mage guild spells", nameEnd + CastleLayout.SpellListsFromNameEnd, CastleLayout.SpellListsLength, ByteRegionKind.Spells, "Five lists of 32-bit spell codes, one per spell level, each 24 bytes apart; the guild shows the first of each as many as its count says"),
                Make("Buildings now", nameEnd + CastleLayout.CurrentBuildingsFromNameEnd, MaskSize, ByteRegionKind.Other, "One bit per building; not the ones an upgrade replaced"),
                Make("Buildings built", nameEnd + CastleLayout.BuiltBuildingsFromNameEnd, MaskSize, ByteRegionKind.Other, "One bit per building, including the ones an upgrade replaced"),
                Make("Buildings allowed", nameEnd + CastleLayout.EnabledBuildingsFromNameEnd, MaskSize, ByteRegionKind.Other, "One bit per building the map allows"),
            };

            return regions.Where(r => r.Length > 0 && r.End <= chunk.Length).OrderBy(r => r.Start).ThenBy(r => r.Length).ToList();
        }

        private static ByteRegion Make(string name, int start, int length, ByteRegionKind kind, string? description = null)
        {
            return new ByteRegion(name, start, length, kind, null, description);
        }

        private static ByteRegion Slots(string name, int start, int count, int size, ByteRegionKind kind, string slotName = "Slot", string? description = null)
        {
            var parts = Enumerable.Range(0, count)
                .Select(i => new ByteRegion($"{slotName} {i + 1}", start + (i * size), size, kind, name))
                .ToList();

            return new ByteRegion(name, start, count * size, kind, null, description, parts);
        }
    }
}
