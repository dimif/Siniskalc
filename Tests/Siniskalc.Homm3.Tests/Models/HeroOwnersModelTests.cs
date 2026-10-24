using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Models
{
    [TestFixture]
    internal sealed class HeroOwnersModelTests
    {
        private static readonly HeroOwnersModel Table = new([HeroOwnership.Free, new HeroOwnership(PlayerColor.Blue, false), HeroOwnership.Reserved]);

        [Test]
        public void AHeroGivenToAPlayerIsNoLongerReserved()
        {
            Assert.That(Table.WithOwner(2, PlayerColor.Red).Heroes[2], Is.EqualTo(new HeroOwnership(PlayerColor.Red, false)));
        }

        [Test]
        public void AHeroThatLeavesItsPlayerIsFree()
        {
            Assert.That(Table.WithOwner(1, PlayerColor.Neutral).Heroes[1], Is.EqualTo(HeroOwnership.Free));
        }

        [Test]
        public void TwoTablesWithTheSameEntriesAreEqual()
        {
            Assert.That(Table.WithOwner(1, PlayerColor.Neutral).WithOwner(1, PlayerColor.Blue), Is.EqualTo(Table));
        }

        [Test]
        public void AHeroIdOutsideTheTableIsRefused()
        {
            Assert.That(Table.Has(3), Is.False);
            Assert.That(() => Table.WithOwner(3, PlayerColor.Red), Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
