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

        private sealed record Skilled(PrimarySkills Skills);

        private sealed record Holder(TestColor Owner, IArmy<Creature> Army);
    }
}
