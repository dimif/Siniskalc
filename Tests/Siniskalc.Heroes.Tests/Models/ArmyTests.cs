namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the army both games share: a fixed number of slots, each a stack or empty.
    /// </summary>
    [TestFixture]
    internal sealed class ArmyTests
    {
        private static readonly Unit Pikeman = new("Pikeman", 1);
        private static readonly Unit Archer = new("Archer", 2);

        [Test]
        public void ANewArmyHasTheGivenNumberOfEmptySlots()
        {
            var army = new Army<Unit>(7);

            Assert.That(army.Slots, Has.Count.EqualTo(7));
            Assert.That(army.Filled, Is.Empty);
        }

        [Test]
        public void PuttingAStackInASlotLeavesTheOriginalArmyAlone()
        {
            var army = new Army<Unit>(5);

            var changed = army.WithSlot(2, Pikeman, 10);

            Assert.That(changed.Slots[2], Is.EqualTo(new ArmySlot<Unit>(Pikeman, 10)));
            Assert.That(army.Slots[2], Is.Null);
        }

        [Test]
        public void SwappingMovesAStackIntoAnEmptySlot()
        {
            var army = new Army<Unit>(5).WithSlot(0, Pikeman, 10).WithSlot(1, Archer, 5);

            var swapped = army.Swap(0, 1);
            var moved = army.Swap(1, 3);

            Assert.That(swapped.Slots.Take(2), Is.EqualTo(new[] { new ArmySlot<Unit>(Archer, 5), new ArmySlot<Unit>(Pikeman, 10) }));
            Assert.That(moved.Slots[1], Is.Null);
            Assert.That(moved.Slots[3], Is.EqualTo(new ArmySlot<Unit>(Archer, 5)));
        }

        [Test]
        public void EmptyingASlotKeepsTheOthers()
        {
            var army = new Army<Unit>(5).WithSlot(0, Pikeman, 10).WithSlot(1, Archer, 5);

            var cleared = army.WithoutSlot(0);

            Assert.That(cleared.Filled, Is.EqualTo(new[] { new ArmySlot<Unit>(Archer, 5) }));
        }

        [Test]
        public void ASlotThatDoesNotExistIsRefused()
        {
            var army = new Army<Unit>(5);

            Assert.That(() => army.WithSlot(5, Pikeman, 1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => army.WithoutSlot(-1), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => army.Swap(0, 5), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ArmiesWithTheSameStacksAreEqual()
        {
            var one = new Army<Unit>(5).WithSlot(0, Pikeman, 10);
            var other = new Army<Unit>(5).WithSlot(0, Pikeman, 10);

            Assert.That(one, Is.EqualTo(other));
            Assert.That(one.GetHashCode(), Is.EqualTo(other.GetHashCode()));
            Assert.That(one, Is.Not.EqualTo(other.WithSlot(0, Pikeman, 11)));
        }

        private sealed record Unit(string Name, byte Code) : IGameEntity;
    }
}
