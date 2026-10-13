namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Where each known field lives inside a record of Heroes of Might and Magic II: a hero, a player or a castle. The offsets are from
    /// the start of the record, so the whole-savegame hex view shifts them to the record and the hex tab of a page uses them as they are.
    /// </summary>
    public static class Homm2RecordRegions
    {
        public static IReadOnlyList<ByteRegion> Hero { get; } = HeroFields();

        public static IReadOnlyList<ByteRegion> Player { get; } = PlayerFields();

        public static IReadOnlyList<ByteRegion> Castle { get; } = CastleFields();

        private static ByteRegion Field(string name, int start, int length, ByteRegionKind kind, string? description = null)
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

        private static ByteRegion Named(string name, int start, ByteRegionKind kind, string description, params (string Name, int Size)[] parts)
        {
            var at = start;
            var elements = new List<ByteRegion>();
            foreach (var (partName, size) in parts)
            {
                elements.Add(new ByteRegion(partName, at, size, kind, name));
                at += size;
            }

            return new ByteRegion(name, start, at - start, kind, null, description, elements);
        }

        private static IReadOnlyList<ByteRegion> HeroFields() =>
        [
            Field("Spell points", Homm2HeroLayout.SpellPoints, 2, ByteRegionKind.Stats, "The spell points the hero has now"),
            Field("Hero number (header)", Homm2HeroLayout.HeaderId, 1, ByteRegionKind.Identity),
            Field("Owner", Homm2HeroLayout.Owner, 1, ByteRegionKind.Identity, "Colour of the player; FF for nobody"),
            Field("Name", Homm2HeroLayout.Name, Homm2HeroLayout.NameLength, ByteRegionKind.Identity, "Text, zero terminated"),
            Field("Hero number", Homm2HeroLayout.Id, 1, ByteRegionKind.Identity),
            Field("Pos X", Homm2HeroLayout.X, 4, ByteRegionKind.Position),
            Field("Pos Y", Homm2HeroLayout.Y, 4, ByteRegionKind.Position),
            Named("Destination", Homm2HeroLayout.Destination, ByteRegionKind.Position, "Where the hero was going when its movement ran out; all FF for none", ("X", 4), ("Y", 4)),
            Field("Max movement", Homm2HeroLayout.MaxMovement, 4, ByteRegionKind.Movement, "Worked out by the game from the army, the skills and the artifacts"),
            Field("Movement left", Homm2HeroLayout.MovementLeft, 4, ByteRegionKind.Movement),
            Field("Experience", Homm2HeroLayout.Experience, 4, ByteRegionKind.Stats),
            Field("Level", Homm2HeroLayout.Level, 1, ByteRegionKind.Stats),
            Named("Primary skills", Homm2HeroLayout.PrimarySkills, ByteRegionKind.Stats, "Attack, defense, spell power and knowledge", ("Attack", 1), ("Defense", 1), ("Spell power", 1), ("Knowledge", 1)),
            Slots("Army creatures", Homm2HeroLayout.ArmyTypes, Constants.ArmySlotCount, 1, ByteRegionKind.Creatures, "Slot", "The creature of each stack; FF for an empty slot"),
            Slots("Army counts", Homm2HeroLayout.ArmyCounts, Constants.ArmySlotCount, 2, ByteRegionKind.Creatures, "Slot", "The number of creatures in each stack"),
            Slots("Skill levels", Homm2HeroLayout.SkillLevels, Constants.Hero.SecondarySkillCount, 1, ByteRegionKind.Skills, "Skill", "0 none, 1 basic, 2 advanced, 3 expert, by the number of the skill"),
            Slots("Skill order", Homm2HeroLayout.SkillOrder, Constants.Hero.SecondarySkillCount, 1, ByteRegionKind.Skills, "Skill", "The place in which the skill was learned (1 for the first), 0 for none"),
            Field("Skill count", Homm2HeroLayout.SkillCount, 1, ByteRegionKind.Skills),
            Slots("Spell book", Homm2HeroLayout.Spells, Homm2HeroLayout.SpellsLength, 1, ByteRegionKind.Spells, "Spell", "1 when the hero knows the spell, by the number of the spell (the first byte is not used)"),
            Slots("Artifacts", Homm2HeroLayout.Artifacts, Constants.Hero.ArtifactSlotCount, 1, ByteRegionKind.Artifacts, "Slot", "The artifact of each slot; FF for an empty slot (81 is the Magic Book)"),
        ];

        private static IReadOnlyList<ByteRegion> PlayerFields() =>
        [
            Field("Hero count", Homm2PlayerLayout.HeroCount, 1, ByteRegionKind.Identity),
            Field("Selected hero", Homm2PlayerLayout.SelectedHero, 1, ByteRegionKind.Other, "Hero number; FF for none"),
            Slots("Heroes", Homm2PlayerLayout.Heroes, Constants.Player.HeroListSize, 1, ByteRegionKind.Identity, "Slot", "Hero numbers; FF for an empty slot"),
            Slots("Tavern heroes", Homm2PlayerLayout.Tavern, Constants.Player.TavernSize, 1, ByteRegionKind.Identity, "Slot", "The two heroes the tavern offers"),
            Field("Days left", Homm2PlayerLayout.DaysLeft, 1, ByteRegionKind.Game, "Days left to own a castle; FF while the player owns one"),
            Field("Castle count", Homm2PlayerLayout.CastleCount, 1, ByteRegionKind.Identity),
            Field("Selected castle", Homm2PlayerLayout.SelectedCastle, 1, ByteRegionKind.Other, "Castle number; FF for none"),
            Slots("Castles", Homm2PlayerLayout.Castles, Constants.Player.CastleListSize, 1, ByteRegionKind.Identity, "Slot", "Castle numbers; FF for an empty slot"),
            Named("Resources", Homm2PlayerLayout.Resources, ByteRegionKind.Resources, "The stock, 32 bits each", ("Wood", 4), ("Mercury", 4), ("Ore", 4), ("Sulfur", 4), ("Crystal", 4), ("Gems", 4), ("Gold", 4)),
            Named("Income", Homm2PlayerLayout.Income, ByteRegionKind.Resources, "What the player earns each day, worked out by the game; 32 bits each", ("Wood", 4), ("Mercury", 4), ("Ore", 4), ("Sulfur", 4), ("Crystal", 4), ("Gems", 4)),
            Field("Gold income", Homm2PlayerLayout.GoldIncome, 4, ByteRegionKind.Resources),
        ];

        private static IReadOnlyList<ByteRegion> CastleFields() =>
        [
            Field("Castle number", Homm2CastleLayout.Index, 1, ByteRegionKind.Identity),
            Field("Owner", Homm2CastleLayout.Owner, 1, ByteRegionKind.Identity, "Colour of the player; FF for nobody"),
            Field("Race", Homm2CastleLayout.Race, 1, ByteRegionKind.Identity, "0 Knight to 5 Necromancer"),
            Field("Pos X", Homm2CastleLayout.X, 1, ByteRegionKind.Position),
            Field("Pos Y", Homm2CastleLayout.Y, 1, ByteRegionKind.Position),
            Named("Shipyard", Homm2CastleLayout.Boat, ByteRegionKind.Position, "Where a ship appears; FF when the castle is not on the coast", ("X", 1), ("Y", 1)),
            Slots("Garrison creatures", Homm2CastleLayout.GarrisonTypes, Constants.ArmySlotCount, 1, ByteRegionKind.Creatures),
            Slots("Garrison counts", Homm2CastleLayout.GarrisonCounts, Constants.ArmySlotCount, 2, ByteRegionKind.Creatures),
            Field("Visiting hero", Homm2CastleLayout.VisitingHero, 1, ByteRegionKind.Identity, "Hero number; FF for none"),
            Field("Town buildings", Homm2CastleLayout.TownBuildings, 2, ByteRegionKind.Other, "One bit each: 01 Mage Guild, 02 tavern, 04 thieves guild, 08 shipyard, 10 well, 20 town only, 40 castle, 80 statue"),
            Field("Dwellings", Homm2CastleLayout.Dwellings, 4, ByteRegionKind.Other, "08 to 100 the six dwellings; 10000 and 20000 the Mage Guild level 1 and 2"),
            Slots("Creatures available", Homm2CastleLayout.Creatures, Constants.Castle.DwellingCount, 2, ByteRegionKind.Creatures, "Dwelling"),
            Slots("Mage Guild spells", Homm2CastleLayout.GuildSpells, Homm2Bytes.GuildSpellSlots, 1, ByteRegionKind.Spells, "Spell", "Four for each of the five levels, each the number of the spell minus one"),
            Slots("Mage Guild spell counts", Homm2CastleLayout.GuildSpellCounts, Constants.Castle.MageGuildLevels, 1, ByteRegionKind.Spells, "Level", "How many of the four spells the guild shows"),
            Field("Name", Homm2CastleLayout.Name, Constants.Castle.NameLength, ByteRegionKind.Identity, "Text, zero terminated"),
        ];
    }
}
