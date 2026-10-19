namespace Siniskalc.Heroes.Tests.Data
{
    using Siniskalc.Heroes.Tests.Entities;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
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

        private sealed record Holder(TestColor Owner, IArmy<Creature> Army);
    }
}
