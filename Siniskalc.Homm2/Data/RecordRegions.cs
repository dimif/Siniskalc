namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Where each known field lives inside a record of Heroes of Might and Magic II: a hero, a player, a castle, and the records of the map
    /// (a mine, a shrine, a sphinx, whose turn it is, the castles that built today and the obelisks). The offsets are from the start of the
    /// record, so the whole-savegame hex view shifts them to the record and the hex tab of a page uses them as they are.
    /// </summary>
    public static class RecordRegions
    {
        public static IReadOnlyList<ByteRegion> Hero { get; } = HeroFields();

        public static IReadOnlyList<ByteRegion> Player { get; } = PlayerFields();

        public static IReadOnlyList<ByteRegion> Castle { get; } = CastleFields();

        /// <summary>
        /// Gets the fields of a record of the list of mines (<see cref="MapObjectLayout.MineTable"/>). The flag of the mine is drawn on a tile of
        /// the map, apart from the record.
        /// </summary>
        public static IReadOnlyList<ByteRegion> Mine { get; } =
        [
            Field("Owner", 0, 1, ByteRegionKind.Identity, "Colour of the player; FF for nobody"),
            Field("Kind", 1, 1, ByteRegionKind.Identity, "0 sawmill, 1 alchemist's lab, 2 ore, 3 sulfur, 4 crystal, 5 gem, 6 gold mine"),
            Field("Pos X", 4, 1, ByteRegionKind.Position),
            Field("Pos Y", 5, 1, ByteRegionKind.Position),
        ];

        /// <summary>
        /// Gets the fields of a shrine: the two bytes of its tile that hold the spell it teaches.
        /// </summary>
        public static IReadOnlyList<ByteRegion> Shrine { get; } =
        [
            Field("Spell", 0, 2, ByteRegionKind.Spells, $"The number of the spell, shifted left by {MapObjectLayout.ShrineSpellShift} bits (the low bits belong to the tile)"),
        ];

        /// <summary>
        /// Gets the fields of the record of whose turn it is.
        /// </summary>
        public static IReadOnlyList<ByteRegion> Turn { get; } =
        [
            Field("Player record", 0, 1, ByteRegionKind.Game, "The record of the player whose turn it is, in the table of players (the humans come first)"),
        ];

        /// <summary>
        /// Gets the fields of the set of the castles that built today (<see cref="MapObjectLayout.BuiltToday"/>).
        /// </summary>
        public static IReadOnlyList<ByteRegion> BuiltToday { get; } =
        [
            Field("Castles", 0, MapObjectLayout.BuiltTodaySize, ByteRegionKind.Other, "A bit for each castle slot: castle 0 is the lowest bit of the first byte"),
        ];

        /// <summary>
        /// Returns the fields of a sphinx whose block is <paramref name="length"/> bytes long (the riddle takes what is left).
        /// </summary>
        public static IReadOnlyList<ByteRegion> Sphinx(int length) =>
        [
            Field("Active", MapObjectLayout.SphinxActive, 1, ByteRegionKind.Game, "1 while the riddle can be answered, 0 once it was"),
            Named("Reward", MapObjectLayout.SphinxReward, ByteRegionKind.Resources, "What an answer gives, 32 bits each", ("Wood", 4), ("Mercury", 4), ("Ore", 4), ("Sulfur", 4), ("Crystal", 4), ("Gems", 4), ("Gold", 4)),
            Field("Artifact", MapObjectLayout.SphinxArtifact, 2, ByteRegionKind.Artifacts, "The artifact an answer gives"),
            Field("Answer count", MapObjectLayout.SphinxAnswerCount, 1, ByteRegionKind.Other),
            Slots("Answers", MapObjectLayout.SphinxAnswers, MapObjectLayout.SphinxMaxAnswers, MapObjectLayout.SphinxAnswerSize, ByteRegionKind.Other, "Answer", "Text, zero terminated"),
            Field("Riddle", MapObjectLayout.SphinxRiddle, System.Math.Max(0, length - MapObjectLayout.SphinxRiddle), ByteRegionKind.Other, "Text, zero terminated"),
        ];

        /// <summary>
        /// Returns the fields of the table of the obelisks (<see cref="ObeliskLayout.Capacity"/> bytes) of a map that has <paramref name="count"/>
        /// of them: a byte each, in the order of the obelisks on the map, and room for the obelisks the map does not have.
        /// </summary>
        public static IReadOnlyList<ByteRegion> Obelisks(int count, int length)
        {
            var regions = new List<ByteRegion>
            {
                Slots("Obelisks", 0, count, 1, ByteRegionKind.Other, "Obelisk", "A bit for each colour that has visited it: 01 Blue, 02 Green, 04 Red, 08 Yellow, 10 Orange, 20 Purple"),
            };
            if (length > count)
            {
                regions.Add(Field("Room for more obelisks", count, length - count, ByteRegionKind.Other, "Not used by this map"));
            }

            return regions;
        }

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
            Field("Spell points", HeroLayout.SpellPoints, 2, ByteRegionKind.Stats, "The spell points the hero has now"),
            Field("Hero number (header)", HeroLayout.HeaderId, 1, ByteRegionKind.Identity),
            Field("Owner", HeroLayout.Owner, 1, ByteRegionKind.Identity, "Colour of the player; FF for nobody"),
            Field("Name", HeroLayout.Name, HeroLayout.NameLength, ByteRegionKind.Identity, "Text, zero terminated"),
            Field("Hero number", HeroLayout.Id, 1, ByteRegionKind.Identity),
            Field("Pos X", HeroLayout.X, 4, ByteRegionKind.Position),
            Field("Pos Y", HeroLayout.Y, 4, ByteRegionKind.Position),
            Field("Facing", HeroLayout.Facing, 1, ByteRegionKind.Position, "0 north, 1 northeast, 2 east, 3 southeast, 4 south, 5 southwest, 6 west, 7 northwest"),
            Named("Destination", HeroLayout.Destination, ByteRegionKind.Position, "The tile the hero is going to (the game marks it on the map); all FF for none", ("X", 4), ("Y", 4)),
            Field("Max movement", HeroLayout.MaxMovement, 4, ByteRegionKind.Movement, "Worked out by the game from the army, the skills and the artifacts"),
            Field("Movement left", HeroLayout.MovementLeft, 4, ByteRegionKind.Movement),
            Field("Experience", HeroLayout.Experience, 4, ByteRegionKind.Stats),
            Field("Level", HeroLayout.Level, 1, ByteRegionKind.Stats),
            Field("Morale, next battle", HeroLayout.BattleMorale, 1, ByteRegionKind.Stats, "A signed byte: what a temple (2) or a visited graveyard (FF, -1) gave for the next battle"),
            Field("Luck, next battle", HeroLayout.BattleLuck, 1, ByteRegionKind.Stats, "A signed byte: what a fountain or a faerie ring (1) gave for the next battle"),
            Field("Remembered effects", HeroLayout.Effects, 4, ByteRegionKind.Other, "Bits: 04 fountain, 08 oasis, 10 faerie ring, 20 graveyard, 0100 temple (cleared with their bonus), 400000 arena, 800000 stables (cleared at the start of a week)"),
            Named("Visited places", HeroLayout.VisitedPlaces, ByteRegionKind.Other, "A bit for each place of its kind the hero has visited", ("Gazebos", 4), ("Forts", 4), ("Witch doctor's huts", 4), ("Mercenary camps", 4), ("Standing stones", 4)),
            Field("Visited Xanadu", HeroLayout.VisitedXanadu, 4, ByteRegionKind.Other, "A bit for each Xanadu the hero has visited"),
            Named("Primary skills", HeroLayout.PrimarySkills, ByteRegionKind.Stats, "Attack, defense, spell power and knowledge", ("Attack", 1), ("Defense", 1), ("Spell power", 1), ("Knowledge", 1)),
            Slots("Army creatures", HeroLayout.ArmyTypes, Constants.ArmySlotCount, 1, ByteRegionKind.Creatures, "Slot", "The creature of each stack; FF for an empty slot"),
            Slots("Army counts", HeroLayout.ArmyCounts, Constants.ArmySlotCount, 2, ByteRegionKind.Creatures, "Slot", "The number of creatures in each stack"),
            Slots("Skill levels", HeroLayout.SkillLevels, Constants.Hero.SecondarySkillCount, 1, ByteRegionKind.Skills, "Skill", "0 none, 1 basic, 2 advanced, 3 expert, by the number of the skill"),
            Slots("Skill order", HeroLayout.SkillOrder, Constants.Hero.SecondarySkillCount, 1, ByteRegionKind.Skills, "Skill", "The place in which the skill was learned (1 for the first), 0 for none"),
            Field("Skill count", HeroLayout.SkillCount, 1, ByteRegionKind.Skills),
            Slots("Spell book", HeroLayout.Spells, HeroLayout.SpellsLength, 1, ByteRegionKind.Spells, "Spell", "1 when the hero knows the spell, by the number of the spell (the first byte is not used)"),
            Slots("Artifacts", HeroLayout.Artifacts, Constants.Hero.ArtifactSlotCount, 1, ByteRegionKind.Artifacts, "Slot", "The artifact of each slot; FF for an empty slot (81 is the Magic Book)"),
        ];

        private static IReadOnlyList<ByteRegion> PlayerFields() =>
        [
            Field("Hero count", PlayerLayout.HeroCount, 1, ByteRegionKind.Identity),
            Field("Selected hero", PlayerLayout.SelectedHero, 1, ByteRegionKind.Other, "Hero number; FF for none"),
            Slots("Heroes", PlayerLayout.Heroes, Constants.Player.HeroListSize, 1, ByteRegionKind.Identity, "Slot", "Hero numbers; FF for an empty slot"),
            Slots("Tavern heroes", PlayerLayout.Tavern, Constants.Player.TavernSize, 1, ByteRegionKind.Identity, "Slot", "The two heroes the tavern offers"),
            Field("Days left", PlayerLayout.DaysLeft, 1, ByteRegionKind.Game, "Days left to own a castle; FF while the player owns one"),
            Field("Castle count", PlayerLayout.CastleCount, 1, ByteRegionKind.Identity),
            Field("Selected castle", PlayerLayout.SelectedCastle, 1, ByteRegionKind.Other, "Castle number; FF for none"),
            Slots("Castles", PlayerLayout.Castles, Constants.Player.CastleListSize, 1, ByteRegionKind.Identity, "Slot", "Castle numbers; FF for an empty slot"),
            Named("Resources", PlayerLayout.Resources, ByteRegionKind.Resources, "The stock, 32 bits each", ("Wood", 4), ("Mercury", 4), ("Ore", 4), ("Sulfur", 4), ("Crystal", 4), ("Gems", 4), ("Gold", 4)),
            Named("Income", PlayerLayout.Income, ByteRegionKind.Resources, "What the player earns each day, worked out by the game; 32 bits each", ("Wood", 4), ("Mercury", 4), ("Ore", 4), ("Sulfur", 4), ("Crystal", 4), ("Gems", 4)),
            Field("Gold income", PlayerLayout.GoldIncome, 4, ByteRegionKind.Resources),
        ];

        private static IReadOnlyList<ByteRegion> CastleFields() =>
        [
            Field("Castle number", CastleLayout.Index, 1, ByteRegionKind.Identity),
            Field("Owner", CastleLayout.Owner, 1, ByteRegionKind.Identity, "Colour of the player; FF for nobody"),
            Field("Faction", CastleLayout.Faction, 1, ByteRegionKind.Identity, "0 Knight to 5 Necromancer"),
            Field("Pos X", CastleLayout.X, 1, ByteRegionKind.Position),
            Field("Pos Y", CastleLayout.Y, 1, ByteRegionKind.Position),
            Named("Shipyard", CastleLayout.Boat, ByteRegionKind.Position, "Where a ship appears; FF when the castle is not on the coast", ("X", 1), ("Y", 1)),
            Slots("Garrison creatures", CastleLayout.GarrisonTypes, Constants.ArmySlotCount, 1, ByteRegionKind.Creatures),
            Slots("Garrison counts", CastleLayout.GarrisonCounts, Constants.ArmySlotCount, 2, ByteRegionKind.Creatures),
            Field("Visiting hero", CastleLayout.VisitingHero, 1, ByteRegionKind.Identity, "Hero number; FF for none"),
            Field("Town buildings", CastleLayout.TownBuildings, 2, ByteRegionKind.Other, "One bit each: 01 Mage Guild, 02 thieves guild, 04 tavern, 08 shipyard, 10 well, 20 town only, 40 castle, 80 statue, 100 left turret, 200 right turret, 400 marketplace, 800 second well (Skull Pile...), 1000 moat, 2000 special building (Library...), 8000 captain's quarters"),
            Field("Dwellings", CastleLayout.Dwellings, 4, ByteRegionKind.Other, "08 to 100 the six dwellings; 10000 and 20000 the Mage Guild level 1 and 2"),
            Slots("Creatures available", CastleLayout.Creatures, Constants.Castle.DwellingCount, 2, ByteRegionKind.Creatures, "Dwelling"),
            Slots("Creatures in upgraded dwellings", CastleLayout.UpgradedCreatures, Constants.Castle.DwellingCount, 2, ByteRegionKind.Creatures, "Place", "Dwellings 2 to 6 once upgraded, then the sixth upgraded twice; their own slot above is then 0"),
            Slots("Mage Guild spells", CastleLayout.GuildSpells, Bytes.GuildSpellSlots, 1, ByteRegionKind.Spells, "Spell", "Four for each of the five levels, each the number of the spell minus one"),
            Slots("Mage Guild spell counts", CastleLayout.GuildSpellCounts, Constants.Castle.MageGuildLevels, 1, ByteRegionKind.Spells, "Level", "How many of the four spells the guild shows"),
            Field("Name", CastleLayout.Name, Constants.Castle.NameLength, ByteRegionKind.Identity, "Text, zero terminated"),
        ];
    }
}
