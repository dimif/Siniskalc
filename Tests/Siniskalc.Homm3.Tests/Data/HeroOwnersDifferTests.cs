using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// The table of hero owners lists no change of its own: the owner of a hero is listed with the hero, and putting it back puts the table
    /// back too.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroOwnersDifferTests
    {
        [Test]
        public void TheTableOfHeroOwnersNeverListsAChangeWhateverIsDifferent()
        {
            var original = new HeroOwnersModel([HeroOwnership.Free, HeroOwnership.Free]);
            var current = new HeroOwnersModel([new HeroOwnership(PlayerColor.Red, false), HeroOwnership.Free]);

            Assert.That(new HeroOwnersDiffer().Diff(0, original, current), Is.Empty);
        }
    }
}
