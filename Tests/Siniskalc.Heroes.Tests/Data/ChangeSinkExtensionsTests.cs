namespace Siniskalc.Heroes.Tests.Data
{
    using Siniskalc.Heroes.Tests.Entities;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using static Siniskalc.Heroes.Tests.Data.ArmySamples;

    /// <summary>
    /// Tests the changes that every game's differs report in the same way.
    /// </summary>
    [TestFixture]
    internal sealed class ChangeSinkExtensionsTests
    {
        private static ChangeSink<Holder> NewSink() =>
            new((change, _) => new PendingChange("Holder", change, new ChunkKind("Holder", 0), 0, _ => { }));

        [Test]
        public void ANewOwnerIsListedWithTheNamesOfTheColoursAndNeutralForNobody()
        {
            var sink = NewSink();

            sink.AddOwner<Holder, TestColor>(TestColor.Neutral, TestColor.Red, (h, owner) => h with { Owner = owner });

            Assert.That(sink.All.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[] { ("Owner", "Neutral", "Red") }));
        }

        [Test]
        public void AnOwnerThatDidNotChangeIsNotListed()
        {
            var sink = NewSink();

            sink.AddOwner<Holder, TestColor>(TestColor.Red, TestColor.Red, (h, owner) => h with { Owner = owner });

            Assert.That(sink.All, Is.Empty);
        }

        private sealed record Player(ResourceSet Resources, TavernHeroes Tavern, int? DaysLeft);

        private static ChangeSink<Player> NewPlayerSink() =>
            new((change, _) => new PendingChange("Player", change, new ChunkKind("Player", 0), 0, _ => { }));

        [Test]
        public void EachResourceThatDiffersIsListedWithItsAmountsBeforeAndAfter()
        {
            var sink = NewPlayerSink();
            var before = new ResourceSet(1, 2, 3, 4, 5, 6, 1500);
            var after = before.With(Resource.Gold, 2500).With(Resource.Ore, 9);

            sink.AddResources(before, after, p => p.Resources, (p, resources) => p with { Resources = resources });

            Assert.That(
                sink.All.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)),
                Is.EquivalentTo(new[] { ("Gold", "1,500", "2,500"), ("Ore", "3", "9") }));
        }

        [Test]
        public void EachPlaceOfTheTavernThatDiffersIsListedWithTheNamesOfTheHeroes()
        {
            var sink = NewPlayerSink();

            sink.AddTavern(new TavernHeroes(4, null), new TavernHeroes(4, 7), id => $"Hero {id}", p => p.Tavern, (p, tavern) => p with { Tavern = tavern });

            Assert.That(sink.All.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[] { ("Tavern hero 2", "(none)", "Hero 7") }));
        }

        [Test]
        public void ChangedDaysLeftAreListedAndNoCountdownReadsAsNotCounting()
        {
            var sink = NewPlayerSink();

            sink.AddDaysLeft(null, 5, (p, days) => p with { DaysLeft = days });

            Assert.That(sink.All.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[] { ("Days left without a castle", "not counting", "5") }));
        }

        [Test]
        public void DaysLeftThatDidNotChangeAreNotListed()
        {
            var sink = NewPlayerSink();

            sink.AddDaysLeft(3, 3, (p, days) => p with { DaysLeft = days });

            Assert.That(sink.All, Is.Empty);
        }

        [Test]
        public void EachSlotThatDiffersIsListedWithItsStackBeforeAndAfter()
        {
            var sink = NewSink();
            var before = new Army<Creature>(5).WithSlot(0, Peasant, 10).WithSlot(2, Archer, 3);
            var after = before.WithSlot(0, Peasant, 12).WithoutSlot(2);

            sink.AddArmy(ChangeAreas.Creatures, before, after, h => h.Army, (h, army) => h with { Army = army });

            Assert.That(
                sink.All.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)),
                Is.EqualTo(new[] { ("Slot 1", "Peasant × 10", "Peasant × 12"), ("Slot 3", "Archer × 3", "empty") }));
        }

        [Test]
        public void EachPrimarySkillThatDiffersIsListedWithItsValuesBeforeAndAfter()
        {
            var sink = new ChangeSink<Skilled>((change, _) => new PendingChange("Skilled", change, new ChunkKind("Skilled", 0), 0, _ => { }));

            sink.AddPrimarySkills(new PrimarySkills(1, 2, 3, 4), new PrimarySkills(1, 5, 3, 7), h => h.Skills, (h, skills) => h with { Skills = skills });

            Assert.That(
                sink.All.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)),
                Is.EqualTo(new[] { ("Defense", "2", "5"), ("Knowledge", "4", "7") }));
        }

        [Test]
        public void APrimarySkillThatWasChangedIsPutBackWithoutTouchingTheOthers()
        {
            var reverts = new List<Func<Skilled, Skilled>>();
            var recording = new ChangeSink<Skilled>((change, revert) =>
            {
                reverts.Add(revert);
                return new PendingChange("Skilled", change, new ChunkKind("Skilled", 0), 0, _ => { });
            });
            recording.AddPrimarySkills(new PrimarySkills(1, 2, 3, 4), new PrimarySkills(9, 2, 3, 4), h => h.Skills, (h, skills) => h with { Skills = skills });

            var putBack = reverts.Single()(new Skilled(new PrimarySkills(9, 8, 7, 6)));

            Assert.That(putBack.Skills, Is.EqualTo(new PrimarySkills(1, 8, 7, 6)));
        }

        [Test]
        public void AChangedLevelIsOneChangeOfTheExperienceWithTheLevelBeforeAndAfter()
        {
            var sink = new ChangeSink<Leveled>((change, _) => new PendingChange("Leveled", change, new ChunkKind("Leveled", 0), 0, _ => { }));

            sink.AddHeroLevel(new HeroLevel(1_000, 2), new HeroLevel(2_000, 3), (h, level) => h with { Level = level });

            Assert.That(sink.All.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[] { ("Experience", "1,000 (level 2)", "2,000 (level 3)") }));
        }

        [Test]
        public void AChangedLevelIsPutBackWithItsExperienceSoTheTwoStayInStep()
        {
            var reverts = new List<Func<Leveled, Leveled>>();
            var recording = new ChangeSink<Leveled>((change, revert) =>
            {
                reverts.Add(revert);
                return new PendingChange("Leveled", change, new ChunkKind("Leveled", 0), 0, _ => { });
            });
            recording.AddHeroLevel(new HeroLevel(1_000, 2), new HeroLevel(2_000, 3), (h, level) => h with { Level = level });

            var putBack = reverts.Single()(new Leveled(new HeroLevel(5_000, 5)));

            Assert.That(putBack.Level, Is.EqualTo(new HeroLevel(1_000, 2)));
        }

        [Test]
        public void ALevelThatDidNotChangeIsNotListed()
        {
            var sink = new ChangeSink<Leveled>((change, _) => new PendingChange("Leveled", change, new ChunkKind("Leveled", 0), 0, _ => { }));

            sink.AddHeroLevel(new HeroLevel(1_000, 2), new HeroLevel(1_000, 2), (h, level) => h with { Level = level });

            Assert.That(sink.All, Is.Empty);
        }

        [Test]
        public void ANewPositionOrFacingIsOneChangeThatShowsThePlaceAndTheFacingBeforeAndAfter()
        {
            var sink = NewMoverSink();

            sink.AddPosition(new HeroPosition(3, 4, FacingDirection.North), new HeroPosition(3, 4, FacingDirection.East), hasLevels: false, (h, position) => h with { Position = position });

            Assert.That(sink.All.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[] { ("Position", "Pos: 3, 4, facing North", "Pos: 3, 4, facing East") }));
        }

        [Test]
        public void APositionSaysItsLevelInAGameWhoseMapHasTwo()
        {
            var sink = NewMoverSink();

            sink.AddPosition(new HeroPosition(3, 4, 0, FacingDirection.North), new HeroPosition(3, 4, 1, FacingDirection.North), hasLevels: true, (h, position) => h with { Position = position });

            Assert.That(
                sink.All.Select(c => (c.Change.Before, c.Change.After)),
                Is.EqualTo(new[] { ("Pos: 3, 4, Surface, facing North", "Pos: 3, 4, Underground, facing North") }));
        }

        [Test]
        public void ADestinationThatWasSetReadsAsNoneBefore()
        {
            var sink = NewMoverSink();

            sink.AddDestination(MapPosition.None, new MapPosition(30, 31), hasLevels: false, (h, destination) => h with { Destination = destination });

            Assert.That(sink.All.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[] { ("Destination", "none", "Pos: 30, 31") }));
        }

        [Test]
        public void ChangedMovementShowsWhatIsLeftAndWhatAFullDayGivesAndIsPutBackWhole()
        {
            var reverts = new List<Func<Mover, Mover>>();
            var sink = new ChangeSink<Mover>((change, revert) =>
            {
                reverts.Add(revert);
                return new PendingChange("Mover", change, new ChunkKind("Mover", 0), 0, _ => { });
            });

            sink.AddMovement(new MovementPoints(320, 1_500), new MovementPoints(1_500, 1_500), (h, movement) => h with { Movement = movement });

            Assert.That(sink.All.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[] { ("Movement points", "320 / 1,500", "1,500 / 1,500") }));
            Assert.That(reverts.Single()(Mover.Nowhere with { Movement = new MovementPoints(9, 9) }).Movement, Is.EqualTo(new MovementPoints(320, 1_500)));
        }

        [Test]
        public void ChangedSpellPointsAreListedWithThePointsBeforeAndAfter()
        {
            var sink = NewMoverSink();

            sink.AddSpellPoints(10, 1_200, (h, points) => h with { SpellPoints = points });

            Assert.That(sink.All.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[] { ("Spell points", "10", "1,200") }));
        }

        [Test]
        public void APlaceMovementOrSpellPointsThatDidNotChangeAreNotListed()
        {
            var sink = NewMoverSink();
            var here = new HeroPosition(3, 4, FacingDirection.North);

            sink.AddPosition(here, here with { }, hasLevels: true, (h, position) => h with { Position = position });
            sink.AddDestination(new MapPosition(1, 2), new MapPosition(1, 2), hasLevels: true, (h, destination) => h with { Destination = destination });
            sink.AddMovement(new MovementPoints(1, 2), new MovementPoints(1, 2), (h, movement) => h with { Movement = movement });
            sink.AddSpellPoints(5, 5, (h, points) => h with { SpellPoints = points });

            Assert.That(sink.All, Is.Empty);
        }

        private static ChangeSink<Mover> NewMoverSink() =>
            new((change, _) => new PendingChange("Mover", change, new ChunkKind("Mover", 0), 0, _ => { }));

        private sealed record Mover(HeroPosition Position, MapPosition Destination, MovementPoints Movement, int SpellPoints)
        {
            public static Mover Nowhere { get; } = new(new HeroPosition(0, 0, FacingDirection.North), MapPosition.None, default, 0);
        }

        private sealed record Leveled(HeroLevel Level);

        private sealed record Skilled(PrimarySkills Skills);

        private sealed record Holder(TestColor Owner, IArmy<Creature> Army);
    }
}
