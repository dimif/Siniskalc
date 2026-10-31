namespace Siniskalc.Heroes.Tests.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Tests.Entities;

    /// <summary>
    /// Tests the changes a differ words for the things every game's records have in common (owner, army, skills, level, position,
    /// resources, tavern...): the area, the description, the text before and after, and the revert that puts the old value back.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ChangeSinkExtensionsTests
    {
        private sealed record Thing(
            TestColor Owner,
            IArmy<ArmySamples.Creature> Army,
            PrimarySkills Skills,
            HeroLevel Level,
            HeroPosition Position,
            MapPosition Destination,
            MovementPoints Movement,
            int SpellPoints,
            ResourceSet Resources,
            TavernHeroes Tavern,
            int? DaysLeft,
            ISkills SecondarySkills,
            SpellSet<TestSpell> Spells);

        private static readonly ChunkKind Kind = new("Thing", 0);

        private static readonly ExperienceTable Table = new([0, 1000, 2000, 3200], 4);

        private static readonly TestSpell Bless = new("Bless", 3);

        private static readonly TestSpell Haste = new("Haste", 1);

        public interface ISkill : ISecondarySkill
        {
        }

        public interface ISkills : ISecondarySkills<ISkill, ISkills>
        {
        }

        private static Thing Blank() => new(
            TestColor.Red,
            new Army<ArmySamples.Creature>(3),
            new PrimarySkills(1, 2, 3, 4),
            HeroLevel.AtLevel(Table, 1),
            new HeroPosition(5, 6, 0, FacingDirection.North),
            new MapPosition(7, 8, 0),
            new MovementPoints(1000, 1500),
            20,
            ResourceSet.None,
            new TavernHeroes(null, null),
            null,
            Mock.Of<ISkills>(),
            SpellSet<TestSpell>.None);

        private static ISkill Skill(string name, byte code) => Mock.Of<ISkill>(s => s.Name == name && s.Code == code);

        /// <summary>
        /// Makes the skills of a hero: <paramref name="slots"/> in their slots, and <paramref name="hidden"/> with a level but no slot.
        /// </summary>
        private static ISkills Skills((ISkill Skill, SkillLevel Level)[] slots, params (ISkill Skill, SkillLevel Level)[] hidden)
        {
            var skills = new Mock<ISkills>();
            skills.SetupGet(s => s.SlotCount).Returns(slots.Length);
            skills.Setup(s => s.GetSkillLevel(It.IsAny<ISkill>())).Returns(SkillLevel.None);
            skills.Setup(s => s.GetSlotOf(It.IsAny<ISkill>())).Returns(-1);
            for (var slot = 0; slot < slots.Length; slot++)
            {
                var (skill, level, index) = (slots[slot].Skill, slots[slot].Level, slot);
                skills.Setup(s => s.GetSkillInSlot(index)).Returns(skill);
                skills.Setup(s => s.GetSlotOf(skill)).Returns(index);
                skills.Setup(s => s.GetSkillLevel(skill)).Returns(level);
            }

            foreach (var (skill, level) in hidden)
            {
                skills.Setup(s => s.GetSkillLevel(skill)).Returns(level);
            }

            return skills.Object;
        }

        private static ChangeSink<Thing> NewSink(out List<Func<Thing, Thing>> reverts)
        {
            var list = new List<Func<Thing, Thing>>();
            reverts = list;
            return new ChangeSink<Thing>((change, revert) =>
            {
                list.Add(revert);
                return new PendingChange("thing", change, Kind, 0, _ => { });
            });
        }

        private static IEnumerable<(string Area, string What, string Before, string After)> Texts(ChangeSink<Thing> sink) =>
            sink.All.Select(c => (c.Change.Area.DisplayName, c.Change.Description, c.Change.Before, c.Change.After));

        [Test]
        public void AnOwnerThatDidNotChangeIsNoChange()
        {
            var sink = NewSink(out _);

            sink.AddOwner(TestColor.Red, TestColor.Red, (t, c) => t with { Owner = c });

            Assert.That(sink.All, Is.Empty);
        }

        [Test]
        public void AnOwnerThatChangedIsWordedWithTheNamesAndRevertsToTheOldOwner()
        {
            var sink = NewSink(out var reverts);

            sink.AddOwner(TestColor.Red, TestColor.Blue, (t, c) => t with { Owner = c });

            Assert.That(Texts(sink), Is.EqualTo(new[] { ("General", "Owner", "Red", "Blue") }));
            Assert.That(reverts[0](Blank() with { Owner = TestColor.Blue }).Owner, Is.EqualTo(TestColor.Red));
        }

        [Test]
        public void TheNextBattleBonusesListOnlyWhatChangedWithASignAndRevertEachOnItsOwn()
        {
            var sink = NewSink(out var reverts);
            var luckPutBack = new List<int>();

            sink.AddNextBattleBonuses(1, -2, 0, 3, (t, morale) => t with { SpellPoints = morale }, (t, luck) =>
            {
                luckPutBack.Add(luck);
                return t;
            });

            Assert.That(Texts(sink), Is.EqualTo(new[] { ("General", "Morale, next battle", "+1", "-2"), ("General", "Luck, next battle", "0", "+3") }));
            Assert.That(reverts[0](Blank() with { SpellPoints = -2 }).SpellPoints, Is.EqualTo(1));
            reverts[1](Blank());
            Assert.That(luckPutBack, Is.EqualTo(new[] { 0 }));
        }

        [Test]
        public void UnchangedNextBattleBonusesListNothing()
        {
            var sink = NewSink(out _);

            sink.AddNextBattleBonuses(1, 1, -1, -1, (t, _) => t, (t, _) => t);

            Assert.That(sink.All, Is.Empty);
        }

        [Test]
        public void PlaceVisitsAreOneChangeForEachKindThatDiffersWithTheNumberOfPlacesAndRevertEachOnItsOwn()
        {
            var sink = NewSink(out var reverts);
            var putBack = new List<(string Kind, uint Mask)>();
            var before = new Dictionary<string, uint> { ["Forts"] = 0b110, ["Gazebos"] = 0b1, ["Xanadu"] = 0 };
            var after = new Dictionary<string, uint> { ["Forts"] = 0b110, ["Gazebos"] = 0b1011, ["Xanadu"] = 0b10 };

            sink.AddPlaceVisits(
                before.Keys,
                kind => kind,
                kind => before[kind],
                kind => after[kind],
                (t, kind, mask) =>
                {
                    putBack.Add((kind, mask));
                    return t;
                });

            Assert.That(Texts(sink), Is.EqualTo(new[] { ("General", "Visited: gazebos", "1", "3"), ("General", "Visited: xanadu", "0", "1") }));
            reverts[1](Blank());
            Assert.That(putBack, Is.EqualTo(new[] { ("Xanadu", 0u) }));
        }

        [Test]
        public void AnArmyChangeIsOneChangeForEachSlotThatDiffersAndEmptySlotsSayEmpty()
        {
            var sink = NewSink(out var reverts);
            var before = new Army<ArmySamples.Creature>(3).WithSlot(0, ArmySamples.Peasant, 10).WithSlot(1, ArmySamples.Archer, 5);
            var after = before.WithSlot(1, null).WithSlot(2, ArmySamples.Archer, 1200);

            sink.AddArmy(ChangeAreas.Creatures, before, after, t => t.Army, (t, a) => t with { Army = a });

            Assert.That(Texts(sink), Is.EqualTo(new[]
            {
                ("Creatures", "Slot 2", "Archer × 5", "empty"),
                ("Creatures", "Slot 3", "empty", "Archer × 1,200"),
            }));
            var reverted = reverts[0](Blank() with { Army = after });
            Assert.That(reverted.Army.Slots[1], Is.EqualTo(before.Slots[1]));
            Assert.That(reverted.Army.Slots[2], Is.EqualTo(after.Slots[2]), "only the slot of the change is put back");
        }

        [Test]
        public void TwoEqualArmiesHaveNoChanges()
        {
            var sink = NewSink(out _);
            var army = new Army<ArmySamples.Creature>(3).WithSlot(0, ArmySamples.Peasant, 10);

            sink.AddArmy(ChangeAreas.Creatures, army, army, t => t.Army, (t, a) => t with { Army = a });

            Assert.That(sink.All, Is.Empty);
        }

        [Test]
        public void ArmiesOfDifferentSizesAreComparedUpToTheShorterOne()
        {
            var sink = NewSink(out _);
            var before = new Army<ArmySamples.Creature>(2).WithSlot(1, ArmySamples.Peasant, 1);
            var after = new Army<ArmySamples.Creature>(4).WithSlot(1, ArmySamples.Peasant, 1).WithSlot(3, ArmySamples.Archer, 9);

            sink.AddArmy(ChangeAreas.Creatures, before, after, t => t.Army, (t, a) => t with { Army = a });

            Assert.That(sink.All, Is.Empty);
        }

        [Test]
        public void ALongerArmyBeforeThanAfterIsComparedUpToTheShorterOneToo()
        {
            var sink = NewSink(out _);
            var before = new Army<ArmySamples.Creature>(4).WithSlot(1, ArmySamples.Peasant, 1).WithSlot(3, ArmySamples.Archer, 9);
            var after = new Army<ArmySamples.Creature>(2).WithSlot(1, ArmySamples.Peasant, 1);

            Assert.That(() => sink.AddArmy(ChangeAreas.Creatures, before, after, t => t.Army, (t, a) => t with { Army = a }), Throws.Nothing);
            Assert.That(sink.All, Is.Empty);
        }

        [Test]
        public void EachPrimarySkillThatChangedIsItsOwnChangeAndRevertsOnlyThatSkill()
        {
            var sink = NewSink(out var reverts);

            sink.AddPrimarySkills(new PrimarySkills(1, 2, 3, 4), new PrimarySkills(9, 2, 8, 7), t => t.Skills, (t, s) => t with { Skills = s });

            Assert.That(Texts(sink), Is.EqualTo(new[]
            {
                ("Primary skills", "Attack", "1", "9"),
                ("Primary skills", "Spell power", "3", "8"),
                ("Primary skills", "Knowledge", "4", "7"),
            }));
            var now = Blank() with { Skills = new PrimarySkills(9, 2, 8, 7) };
            Assert.That(reverts[0](now).Skills, Is.EqualTo(new PrimarySkills(1, 2, 8, 7)));
            Assert.That(reverts[1](now).Skills, Is.EqualTo(new PrimarySkills(9, 2, 3, 7)));
            Assert.That(reverts[2](now).Skills, Is.EqualTo(new PrimarySkills(9, 2, 8, 4)));
        }

        [Test]
        public void ADefenceThatChangedIsListedAsDefense()
        {
            var sink = NewSink(out var reverts);

            sink.AddPrimarySkills(new PrimarySkills(1, 2, 3, 4), new PrimarySkills(1, 5, 3, 4), t => t.Skills, (t, s) => t with { Skills = s });

            Assert.That(Texts(sink), Is.EqualTo(new[] { ("Primary skills", "Defense", "2", "5") }));
            Assert.That(reverts[0](Blank() with { Skills = new PrimarySkills(1, 5, 3, 4) }).Skills.Defense, Is.EqualTo(2));
        }

        [Test]
        public void TheLevelIsWordedWithTheExperienceAndTheLevel()
        {
            var sink = NewSink(out var reverts);
            var before = HeroLevel.AtLevel(Table, 1);
            var after = HeroLevel.AtExperience(Table, 2500);

            sink.AddHeroLevel(before, after, (t, l) => t with { Level = l });
            sink.AddHeroLevel(before, before, (t, l) => t with { Level = l });

            Assert.That(Texts(sink), Is.EqualTo(new[] { ("General", "Experience", "0 (level 1)", "2,500 (level 3)") }));
            Assert.That(reverts[0](Blank() with { Level = after }).Level, Is.EqualTo(before));
        }

        [Test]
        public void ThePositionIsWordedWithTheTileTheLevelAndTheFacingAccordingToTheLevelsOfTheMap()
        {
            var before = new HeroPosition(5, 6, 0, FacingDirection.North);
            var after = new HeroPosition(9, 10, 1, FacingDirection.South);
            var withLevels = NewSink(out var reverts);
            var withoutLevels = NewSink(out _);

            withLevels.AddPosition(before, after, true, (t, p) => t with { Position = p });
            withoutLevels.AddPosition(before, after, false, (t, p) => t with { Position = p });
            withoutLevels.AddPosition(before, before, false, (t, p) => t with { Position = p });

            Assert.That(Texts(withLevels), Is.EqualTo(new[] { ("General", "Position", "at 5, 6, Surface, facing North", "at 9, 10, Underground, facing South") }));
            Assert.That(Texts(withoutLevels), Is.EqualTo(new[] { ("General", "Position", "at 5, 6, facing North", "at 9, 10, facing South") }));
            Assert.That(reverts[0](Blank() with { Position = after }).Position, Is.EqualTo(before));
        }

        [Test]
        public void ADestinationOfNoneIsWordedAsNone()
        {
            var sink = NewSink(out var reverts);

            sink.AddDestination(MapPosition.None, new MapPosition(3, 4, 0), true, (t, d) => t with { Destination = d });
            sink.AddDestination(MapPosition.None, MapPosition.None, true, (t, d) => t with { Destination = d });

            Assert.That(Texts(sink), Is.EqualTo(new[] { ("General", "Destination", "none", "at 3, 4, Surface") }));
            Assert.That(reverts[0](Blank()).Destination, Is.EqualTo(MapPosition.None));
        }

        [Test]
        public void MovementPointsAreWordedAsCurrentOverMaximum()
        {
            var sink = NewSink(out var reverts);

            sink.AddMovement(new MovementPoints(1000, 1500), new MovementPoints(0, 2000), (t, m) => t with { Movement = m });
            sink.AddMovement(new MovementPoints(1, 1), new MovementPoints(1, 1), (t, m) => t with { Movement = m });

            Assert.That(Texts(sink), Is.EqualTo(new[] { ("General", "Movement points", "1,000 / 1,500", "0 / 2,000") }));
            Assert.That(reverts[0](Blank()).Movement, Is.EqualTo(new MovementPoints(1000, 1500)));
        }

        [Test]
        public void SpellPointsAreWordedWithThousandsSeparators()
        {
            var sink = NewSink(out var reverts);

            sink.AddSpellPoints(20, 1234, (t, s) => t with { SpellPoints = s });
            sink.AddSpellPoints(7, 7, (t, s) => t with { SpellPoints = s });

            Assert.That(Texts(sink), Is.EqualTo(new[] { ("General", "Spell points", "20", "1,234") }));
            Assert.That(reverts[0](Blank() with { SpellPoints = 1234 }).SpellPoints, Is.EqualTo(20));
        }

        [Test]
        public void EachResourceThatChangedIsItsOwnChangeAndRevertsOnlyThatResource()
        {
            var sink = NewSink(out var reverts);
            var before = ResourceSet.None.With(Resource.Gold, 10000).With(Resource.Wood, 5);
            var after = before.With(Resource.Gold, 25000).With(Resource.Gems, 2);

            sink.AddResources(before, after, t => t.Resources, (t, r) => t with { Resources = r });

            Assert.That(Texts(sink).Select(t => (t.Area, t.What, t.Before, t.After)), Is.EquivalentTo(new[]
            {
                ("Resources", "Gems", "0", "2"),
                ("Resources", "Gold", "10,000", "25,000"),
            }));
            var gold = sink.All.ToList().FindIndex(c => c.Change.Description == "Gold");
            var reverted = reverts[gold](Blank() with { Resources = after });
            Assert.That((reverted.Resources[Resource.Gold], reverted.Resources[Resource.Gems]), Is.EqualTo((10000L, 2L)));
        }

        [Test]
        public void ATavernHeroThatChangedIsNamedByTheFunctionAndNoHeroIsWordedAsNone()
        {
            var sink = NewSink(out var reverts);

            sink.AddTavern(new TavernHeroes(4, null), new TavernHeroes(null, 9), id => $"Hero {id}", t => t.Tavern, (t, v) => t with { Tavern = v });

            Assert.That(Texts(sink), Is.EqualTo(new[]
            {
                ("General", "Tavern hero 1", "Hero 4", "(none)"),
                ("General", "Tavern hero 2", "(none)", "Hero 9"),
            }));
            var reverted = reverts[0](Blank() with { Tavern = new TavernHeroes(null, 9) });
            Assert.That(reverted.Tavern.Ids, Is.EqualTo(new int?[] { 4, 9 }));
        }

        [Test]
        public void DaysLeftWithoutACastleAreWordedAsADayCountOrAsNotCounting()
        {
            var sink = NewSink(out var reverts);

            sink.AddDaysLeft(null, 5, (t, d) => t with { DaysLeft = d });
            sink.AddDaysLeft(5, null, (t, d) => t with { DaysLeft = d });
            sink.AddDaysLeft(3, 3, (t, d) => t with { DaysLeft = d });

            Assert.That(Texts(sink), Is.EqualTo(new[]
            {
                ("General", "Days left without a castle", "not counting", "5"),
                ("General", "Days left without a castle", "5", "not counting"),
            }));
            Assert.That(reverts[0](Blank() with { DaysLeft = 5 }).DaysLeft, Is.Null);
            Assert.That(reverts[1](Blank()).DaysLeft, Is.EqualTo(5));
        }

        [Test]
        public void TheSecondarySkillsAreOneChangeListingTheSlotsInOrderAndPuttingTheWholeSetBack()
        {
            var sink = NewSink(out var reverts);
            var (archery, wisdom) = (Skill("Archery", 1), Skill("Wisdom", 7));
            var before = Skills([(archery, SkillLevel.Beginner), (wisdom, SkillLevel.Expert)]);
            var after = Skills([(wisdom, SkillLevel.Expert), (archery, SkillLevel.Beginner)]);

            sink.AddSecondarySkills([archery, wisdom], before, after, (t, s) => t with { SecondarySkills = s });

            Assert.That(Texts(sink), Is.EqualTo(new[] { ("Secondary skills", "Skills", "Archery (Beginner), Wisdom (Expert)", "Wisdom (Expert), Archery (Beginner)") }));
            Assert.That(reverts[0](Blank() with { SecondarySkills = after }).SecondarySkills, Is.SameAs(before));
        }

        [Test]
        public void SecondarySkillsThatReadTheSameAreNoChange()
        {
            var sink = NewSink(out _);
            var archery = Skill("Archery", 1);

            sink.AddSecondarySkills([archery], Skills([(archery, SkillLevel.Expert)]), Skills([(archery, SkillLevel.Expert)]), (t, s) => t with { SecondarySkills = s });

            Assert.That(sink.All, Is.Empty);
        }

        [Test]
        public void NoSecondarySkillIsWordedAsNoneAndASkillWithALevelButNoSlotAsNotShown()
        {
            var sink = NewSink(out _);
            var (archery, luck, wisdom) = (Skill("Archery", 1), Skill("Luck", 9), Skill("Wisdom", 7));

            sink.AddSecondarySkills(
                [archery, luck, wisdom],
                Skills([]),
                Skills([(archery, SkillLevel.None)], (luck, SkillLevel.Expert), (wisdom, SkillLevel.Beginner)),
                (t, s) => t with { SecondarySkills = s });

            Assert.That(Texts(sink).Single().After, Is.EqualTo("Archery (no level), not shown: Wisdom (Beginner), Luck (Expert)"));
            Assert.That(Texts(sink).Single().Before, Is.EqualTo("none"));
        }

        [Test]
        public void EachSpellLearnedOrForgottenIsItsOwnChangeInTheOrderOfTheCodesAndRevertsOnlyThatSpell()
        {
            var sink = NewSink(out var reverts);
            var before = new SpellSet<TestSpell>([Bless]);
            var after = new SpellSet<TestSpell>([Haste]);

            sink.AddLearnedSpells(before, after, (t, spell, learned) => t with { Spells = learned ? t.Spells.With(spell) : t.Spells.Without(spell) });

            Assert.That(Texts(sink), Is.EqualTo(new[]
            {
                ("Spells", "Haste", "not learned", "learned"),
                ("Spells", "Bless", "learned", "not learned"),
            }));
            Assert.That(reverts[1](Blank() with { Spells = after }).Spells, Is.EqualTo(new SpellSet<TestSpell>([Bless, Haste])));
            Assert.That(reverts[0](Blank() with { Spells = after }).Spells, Is.EqualTo(SpellSet<TestSpell>.None));
        }

        [Test]
        public void TheSameLearnedSpellsAreNoChange()
        {
            var sink = NewSink(out _);

            sink.AddLearnedSpells(new SpellSet<TestSpell>([Bless]), new SpellSet<TestSpell>([Bless]), (t, spell, learned) => t);

            Assert.That(sink.All, Is.Empty);
        }

        private sealed record TestSpell(string Name, byte Code) : IGameEntity;
    }
}
