namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;

    /// <summary>
    /// Describes where each known field lives in a hero chunk, using the real sizes the
    /// <see cref="SafeHeroSerializer"/> and <see cref="HeroDeserializer"/> read and write.
    /// </summary>
    public sealed class HeroRegionProvider : IHeroRegionProvider
    {
        /// <summary>
        /// The hero name is stored in 13 bytes, up to the secondary skills.
        /// </summary>
        public const int NameLength = 13;

        private const int ArtifactSlotSize = 8;

        private readonly IVersionSpecificEntitiesProviderFactory versionEntities;

        public HeroRegionProvider(IVersionSpecificEntitiesProviderFactory versionEntities)
        {
            this.versionEntities = versionEntities;
        }

        /// <summary>
        /// Returns a region for every <see cref="HeroOffsetName"/> that exists in the game version, ordered by offset.
        /// Regions that would run past <paramref name="chunkLength"/> are left out.
        /// </summary>
        public IReadOnlyList<ByteRegion> GetRegions(HeroOffsets offsets, int chunkLength = int.MaxValue)
        {
            var entities = this.versionEntities.For(offsets.GameVersion);
            var skillBytes = entities.SecondarySkills.Max(s => s.Code) + 1;
            var spellBytes = entities.Spells.Max(s => s.Code) + 1;
            var regions = new List<ByteRegion>();

            foreach (var name in Enum.GetValues<HeroOffsetName>())
            {
                if (name == HeroOffsetName.Cannon && !entities.WarMachines.Contains(WarMachine.Cannon))
                {
                    continue;
                }

                if (name == HeroOffsetName.Item5 && !offsets.HasItem5)
                {
                    continue;
                }

                var start = offsets[name];
                var region = name switch
                {
                    HeroOffsetName.PositionX => Make("Map X", start, 2, ByteRegionKind.Position),
                    HeroOffsetName.PositionY => Make("Map Y", start, 2, ByteRegionKind.Position),
                    HeroOffsetName.PositionZ => Make("Map level", start, 2, ByteRegionKind.Position),
                    HeroOffsetName.Owner => Make("Owner", start, 1, ByteRegionKind.Identity, "PlayerColor colour; FF means none"),
                    HeroOffsetName.MovedToday => Make("Moved today", start, 1, ByteRegionKind.Movement, "1 once the hero has moved today, back to 0 on a new day. Probably what the game checks before the hero may dig for the grail"),
                    HeroOffsetName.ActiveSpells => new ByteRegion("Active adventure spells", start, 5, ByteRegionKind.Spells, null, "One byte per spell; 0 when the spell was cast today, 255 when not. A new day clears them. The fourth byte has been 0 in every savefile", ActiveSpellParts(start)),
                    HeroOffsetName.MoraleBonus => Make("Morale bonus", start, 1, ByteRegionKind.Stats, "Signed: what visited buildings gave (watering hole +1, crypt -1) until the next battle"),
                    HeroOffsetName.LuckBonus => Make("Luck bonus", start, 1, ByteRegionKind.Stats, "Signed: what visited buildings gave (swan pond +2, fountain of fortune +1) until the next battle"),
                    HeroOffsetName.VisitEffects => new ByteRegion("Visited building effects", start, 4, ByteRegionKind.Movement, null, "Bits of the buildings the hero visited; a battle clears some of them. The third byte has not been seen to change", VisitEffectParts(start)),
                    HeroOffsetName.Class => Make("Class", start, 1, ByteRegionKind.Identity),
                    HeroOffsetName.Facing => Make("Facing", start, 1, ByteRegionKind.Position),
                    HeroOffsetName.Gender => Make("Gender", start, 1, ByteRegionKind.Identity, "0 is male, 1 is female. Part of the header in front of the biography, so not in the chunk of a hero with a biography"),
                    HeroOffsetName.HeroId => Make("Hero id", start, 1, ByteRegionKind.Identity, "The position of the hero in the table of heroes of the savefile; castles refer to heroes by it"),
                    HeroOffsetName.Portrait => Make("Portrait", start, 1, ByteRegionKind.Identity, "Equal to the hero id unless the map gives the hero another portrait"),
                    HeroOffsetName.MarkerX => Make("Marker X", start, 2, ByteRegionKind.Position),
                    HeroOffsetName.MarkerY => Make("Marker Y", start, 2, ByteRegionKind.Position),
                    HeroOffsetName.MarkerZ => Make("Marker level", start, 2, ByteRegionKind.Position),
                    HeroOffsetName.MaxMovementPoints => Make("Movement points, maximum", start, 4, ByteRegionKind.Movement),
                    HeroOffsetName.CurrentMovementPoints => Make("Movement points, left", start, 4, ByteRegionKind.Movement),
                    HeroOffsetName.ExperiencePoints => Make("Experience", start, 4, ByteRegionKind.Stats),
                    HeroOffsetName.NumOfSkills => Make("Number of skills", start, 1, ByteRegionKind.Skills),
                    HeroOffsetName.CurrentSpellPoints => Make("Spell points", start, 2, ByteRegionKind.Spells),
                    HeroOffsetName.Level => Make("Level", start, 1, ByteRegionKind.Stats),
                    HeroOffsetName.Creatures => Slots("Creature types", start, Constants.CreatureSlotCount, 4, "Slot", ByteRegionKind.Creatures),
                    HeroOffsetName.CreaturesAmounts => Slots("Creature counts", start, Constants.CreatureSlotCount, 4, "Slot", ByteRegionKind.Creatures),
                    HeroOffsetName.Name => Make("Name", start, NameLength, ByteRegionKind.Identity, "Text, padded with zeros"),
                    HeroOffsetName.Skills => Table("Skill levels", start, entities.SecondarySkills.Select(s => (s.Name, (int)s.Code)), skillBytes, ByteRegionKind.Skills),
                    HeroOffsetName.SkillSlots => Table("Skill slots", start, entities.SecondarySkills.Select(s => (s.Name, (int)s.Code)), skillBytes, ByteRegionKind.Skills),
                    HeroOffsetName.AttributeAtk => Make("Attack", start, 1, ByteRegionKind.Stats),
                    HeroOffsetName.AttributeDef => Make("Defence", start, 1, ByteRegionKind.Stats),
                    HeroOffsetName.AttributePow => Make("Spell power", start, 1, ByteRegionKind.Stats),
                    HeroOffsetName.AttributeKno => Make("Knowledge", start, 1, ByteRegionKind.Stats),
                    HeroOffsetName.SpellBookSpells => Make("Spells in book", start, spellBytes, ByteRegionKind.Spells, "One byte per spell; 1 means learned"),
                    HeroOffsetName.AvailableSpells => Make("Spells from artifacts", start, spellBytes, ByteRegionKind.Spells, "One byte per spell; 1 means available"),
                    HeroOffsetName.Ballista => Make("Ballista", start, ArtifactSlotSize, ByteRegionKind.WarMachines),
                    HeroOffsetName.Cannon => Make("Cannon", start, ArtifactSlotSize, ByteRegionKind.WarMachines, "Shares its place with the ballista"),
                    HeroOffsetName.AmmoCart => Make("Ammo cart", start, ArtifactSlotSize, ByteRegionKind.WarMachines),
                    HeroOffsetName.FirstAidTent => Make("First aid tent", start, ArtifactSlotSize, ByteRegionKind.WarMachines),
                    HeroOffsetName.SpellBook => Make("Spell book", start, ArtifactSlotSize, ByteRegionKind.Artifacts),
                    HeroOffsetName.Backpack => Slots("Backpack", start, Constants.BackpackSlotCount, Constants.BackpackSlotSize, "Slot", ByteRegionKind.Artifacts),
                    _ => Make(SlotName(name), start, ArtifactSlotSize, ByteRegionKind.Artifacts),
                };

                if (region.End <= chunkLength)
                {
                    regions.Add(region);
                }
            }

            if (offsets.AsleepOffset is { } asleep && asleep + 1 <= chunkLength)
            {
                regions.Add(Make("Asleep", asleep, 1, ByteRegionKind.Movement, "1 for a hero that was put to sleep, 0 when it is awake. A hero that moves wakes up. After the hero struct in Shadow of Death, inside the chunk in Horn of the Abyss; the other versions were not seen"));
            }

            if (offsets.ReservedSlotCounters.Count > 0)
            {
                var reserved = ReservedSlots(offsets.ReservedSlotCounters);
                if (reserved.End <= chunkLength)
                {
                    regions.Add(reserved);
                }
            }

            return regions.OrderBy(r => r.Start).ThenBy(r => r.Length).ToList();
        }

        /// <summary>
        /// Returns the region of the hero header: what sits in front of a hero with a biography, so it is not in the chunk. The
        /// offsets are from <see cref="HeroChunk.HeaderIndex"/>. The position is the same one the chunk of a hero without a
        /// biography starts with.
        /// </summary>
        public IReadOnlyList<ByteRegion> GetHeaderRegions(HeroOffsets offsets)
        {
            return
            [
                Make("Map X", offsets[HeroOffsetName.PositionX], 2, ByteRegionKind.Position),
                Make("Map Y", offsets[HeroOffsetName.PositionY], 2, ByteRegionKind.Position),
                Make("Map level", offsets[HeroOffsetName.PositionZ], 2, ByteRegionKind.Position),
                Make("Gender", offsets[HeroOffsetName.Gender], 1, ByteRegionKind.Identity, "0 is male, 1 is female"),
                Make("Biography length", HeroChunk.BiographyLengthOffset, 4, ByteRegionKind.Identity, "The number of bytes of the biography that follows the header"),
            ];
        }

        private static ByteRegion ReservedSlots(IReadOnlyList<(ArtifactSlots Group, int Offset)> counters)
        {
            const string name = "Reserved slots";
            var parts = counters
                .Select(c => new ByteRegion(SlotGroupName(c.Group), c.Offset, 1, ByteRegionKind.Artifacts, name))
                .ToList();
            var start = parts.Min(p => p.Start);

            return new ByteRegion(name, start, parts.Max(p => p.End) - start, ByteRegionKind.Artifacts, null, "How many slots of the group a combination artifact takes besides the one it is worn in", parts);
        }

        private static string SlotGroupName(ArtifactSlots group) => group switch
        {
            ArtifactSlots.Rings => "Rings",
            ArtifactSlots.Misc => "Misc",
            _ => group.ToString(),
        };

        private static IReadOnlyList<ByteRegion> VisitEffectParts(int start)
        {
            const string group = "Visited building effects";
            return
            [
                new ByteRegion("Stables 0x02, swan pond 0x08, watering hole 0x40", start, 1, ByteRegionKind.Movement, group),
                new ByteRegion("Crypt 0x04, faerie ring 0x20", start + 1, 1, ByteRegionKind.Movement, group),
                new ByteRegion("Unknown", start + 2, 1, ByteRegionKind.Movement, group),
                new ByteRegion("Idol of fortune 0x02, fountain of fortune 0x08", start + 3, 1, ByteRegionKind.Movement, group),
            ];
        }

        private static IReadOnlyList<ByteRegion> ActiveSpellParts(int start)
        {
            const string group = "Active adventure spells";
            return
            [
                new ByteRegion("Disguise", start, 1, ByteRegionKind.Spells, group),
                new ByteRegion("Fly", start + 1, 1, ByteRegionKind.Spells, group),
                new ByteRegion("Water Walk", start + 2, 1, ByteRegionKind.Spells, group),
                new ByteRegion("Unknown, always 0", start + 3, 1, ByteRegionKind.Spells, group),
                new ByteRegion("Visions", start + 4, 1, ByteRegionKind.Spells, group),
            ];
        }

        private static string SlotName(HeroOffsetName name) => name switch
        {
            HeroOffsetName.LeftRing => "Left ring",
            HeroOffsetName.RightRing => "Right ring",
            HeroOffsetName.Item1 => "Misc 1",
            HeroOffsetName.Item2 => "Misc 2",
            HeroOffsetName.Item3 => "Misc 3",
            HeroOffsetName.Item4 => "Misc 4",
            HeroOffsetName.Item5 => "Misc 5",
            _ => name.ToString(),
        };

        private static ByteRegion Make(string name, int start, int length, ByteRegionKind kind, string? description = null)
        {
            return new ByteRegion(name, start, length, kind, null, description);
        }

        private static ByteRegion Slots(string name, int start, int count, int size, string slotName, ByteRegionKind kind)
        {
            var parts = Enumerable.Range(0, count)
                .Select(i => new ByteRegion($"{slotName} {i + 1}", start + (i * size), size, kind, name))
                .ToList();

            return new ByteRegion(name, start, count * size, kind, null, null, parts);
        }

        private static ByteRegion Table(string name, int start, IEnumerable<(string Name, int Code)> entries, int length, ByteRegionKind kind)
        {
            var parts = entries
                .OrderBy(e => e.Code)
                .Select(e => new ByteRegion(e.Name, start + e.Code, 1, kind, name))
                .ToList();

            return new ByteRegion(name, start, length, kind, null, "One byte per skill", parts);
        }
    }
}
