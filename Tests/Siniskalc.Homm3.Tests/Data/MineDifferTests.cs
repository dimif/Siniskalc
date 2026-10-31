using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// What the differ of the mines of the map lists, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MineDifferTests
    {
        private static MineModel Mine() => new(2, MineKind.GoldMine, PlayerColor.Blue, new MapPosition(14, 15, MapPosition.Underground), new Army<Creature>(Constants.CreatureSlotCount));

        [Test]
        public void TwoEqualMinesHaveNoChanges()
        {
            Assert.That(new MineDiffer().Diff(0, Mine(), Mine()), Is.Empty);
        }

        [Test]
        public void ANewOwnerOfAMineIsOneChangeOfThePlaceNamedByTheMineWithItsLevel()
        {
            var change = new MineDiffer().Diff(2, Mine(), Mine() with { Owner = PlayerColor.Neutral }).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Owner", "Blue", "Neutral")));
            Assert.That((change.OwnerName, change.Kind, change.Index), Is.EqualTo((Mine().Name, Homm3ChunkKinds.Mines, 2)));
            Assert.That(change.OwnerName, Does.Contain("Underground"));
        }

        [Test]
        public void AStackOfTheGuardIsAChangeOfItsSlotUnderTheGarrison()
        {
            var original = Mine();
            var current = original with { Guard = original.Guard.WithSlot(3, Creature.Pikeman, 40) };

            var change = new MineDiffer().Diff(2, original, current).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.Area), Is.EqualTo(("Slot 4", ChangeText.Empty, ChangeAreas.Garrison)));
        }

        [Test]
        public void TheOwnerIsListedBeforeTheGuard()
        {
            var original = Mine();
            var current = (original with { Owner = PlayerColor.Red }) with { Guard = original.Guard.WithSlot(0, Creature.Pikeman, 1) };

            Assert.That(new MineDiffer().Diff(0, original, current).Select(c => c.Change.Description), Is.EqualTo(new[] { "Owner", "Slot 1" }));
        }
    }
}
