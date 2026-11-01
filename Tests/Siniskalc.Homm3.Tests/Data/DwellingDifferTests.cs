using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// What the differ of the dwellings of the map lists, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class DwellingDifferTests
    {
        private static DwellingModel Dwelling() => new(1, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Pikeman").Code, TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Pikeman"), PlayerColor.Neutral, new MapPosition(7, 8), 14);

        [Test]
        public void TwoEqualDwellingsHaveNoChanges()
        {
            Assert.That(new DwellingDiffer().Diff(0, Dwelling(), Dwelling()), Is.Empty);
        }

        [Test]
        public void ANewOwnerIsOneChangeOfThePlaceNamedByTheDwelling()
        {
            var change = new DwellingDiffer().Diff(1, Dwelling(), Dwelling() with { Owner = PlayerColor.Red }).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Owner", "Neutral", "Red")));
            Assert.That((change.OwnerName, change.Kind, change.Index), Is.EqualTo((Dwelling().Name, Homm3ChunkKinds.Dwellings, 1)));
        }

        [Test]
        public void ADifferentNumberOfCreaturesToHireIsOneChange()
        {
            var change = new DwellingDiffer().Diff(1, Dwelling(), Dwelling() with { Stock = 20 }).Single();

            Assert.That((change.Change.Description, change.Change.Before, change.Change.After), Is.EqualTo(("Creatures to hire", "14", "20")));
        }

        [Test]
        public void ADwellingOfACreatureTheGameDoesNotHaveIsNamedByItsCode()
        {
            var unknown = Dwelling() with { CreatureCode = 200, Creature = null };

            var change = new DwellingDiffer().Diff(1, unknown, unknown with { Stock = 1 }).Single();

            Assert.That(change.OwnerName, Does.StartWith("Creature 200 dwelling"));
        }
    }
}
