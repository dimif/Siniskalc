namespace Siniskalc.Heroes.Tests.Models
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Tests the two heroes a tavern offers.
    /// </summary>
    [TestFixture]
    internal sealed class TavernHeroesTests
    {
        [Test]
        public void ATavernKeepsTheHeroOfEachPlaceAndFindsThem()
        {
            var tavern = TavernHeroes.Empty.With(0, 12).With(1, 40);

            Assert.That(tavern.Ids, Is.EqualTo(new int?[] { 12, 40 }));
            Assert.That(tavern.Contains(40), Is.True);
            Assert.That(tavern.Contains(41), Is.False);
            Assert.That(tavern.With(0, null).Ids, Is.EqualTo(new int?[] { null, 40 }));
        }

        [Test]
        public void AHeroNumberIsAByteBelow255AndThereAreTwoPlaces()
        {
            Assert.That(TavernHeroes.Empty.With(0, 254).First, Is.EqualTo(254));
            Assert.Throws<ArgumentOutOfRangeException>(() => TavernHeroes.Empty.With(0, 255));
            Assert.Throws<ArgumentOutOfRangeException>(() => TavernHeroes.Empty.With(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => TavernHeroes.Empty.With(2, 1));
        }
    }
}
