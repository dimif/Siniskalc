using Siniskalc.Core.Models;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Models
{
    /// <summary>
    /// The reducers of the list of heroes of a player, as the game changes it in the saves of <c>TestData/Hota/HeroOwners</c>.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerModelTests
    {
        private static PlayerModel Player(int? selected, params int[] heroIds) =>
            new(PlayerColor.Blue, new ResourceSet(0, 0, 0, 0, 0, 0, 0), new TavernHeroes(null, null), new Numbers(heroIds), selected);

        [Test]
        public void LosingAHeroMovesTheHeroesAfterItUpAPlace()
        {
            var red = Player(null, 115, 36, 55, 84, 102, 17, 144).WithoutHero(17);

            Assert.That(red.HeroIds, Is.EqualTo(Numbers.Of(115, 36, 55, 84, 102, 144)));
            Assert.That(red.SelectedHero, Is.Null);
        }

        [Test]
        public void LosingTheSelectedHeroSelectsTheFirstHeroLeft()
        {
            var blue = Player(22, 0, 22).WithoutHero(22);

            Assert.That(blue.HeroIds, Is.EqualTo(Numbers.Of(0)));
            Assert.That(blue.SelectedHero, Is.EqualTo(0));
        }

        [Test]
        public void LosingTheLastHeroLeavesNoHeroSelected()
        {
            var blue = Player(22, 22).WithoutHero(22);

            Assert.That(blue.HeroIds, Is.Empty);
            Assert.That(blue.SelectedHero, Is.Null);
        }

        [Test]
        public void LosingAHeroThatIsNotInTheListChangesNothing()
        {
            var blue = Player(0, 0, 22);

            Assert.That(blue.WithoutHero(29), Is.SameAs(blue));
        }

        [Test]
        public void GainingAHeroPutsItAtTheEndOfTheListAndKeepsTheSelectedHero()
        {
            var red = Player(null, 115, 36).WithHero(65);

            Assert.That(red.HeroIds, Is.EqualTo(Numbers.Of(115, 36, 65)));
            Assert.That(red.SelectedHero, Is.Null);
        }

        [Test]
        public void APlayerWithEightHeroesCannotGainAnother()
        {
            var blue = Player(null, 0, 169, 164, 184, 129, 70, 71, 10);

            Assert.That(() => blue.WithHero(29), Throws.TypeOf<GameRuleException>());
        }

        [Test]
        public void PuttingALostHeroBackReturnsItToItsPlaceAndItsSelection()
        {
            var original = Player(22, 0, 22);

            Assert.That(original.WithoutHero(22).WithHeroAsIn(22, original), Is.EqualTo(original));
        }

        [Test]
        public void PuttingAGainedHeroBackTakesItOutOfTheList()
        {
            var original = Player(null, 115, 36);

            Assert.That(original.WithHero(65).WithHeroAsIn(65, original), Is.EqualTo(original));
        }

        [Test]
        public void PuttingOneHeroBackLeavesTheOtherChangesOfTheList()
        {
            var original = Player(null, 115, 36, 55, 84);
            var edited = original.WithoutHero(36).WithoutHero(84).WithHero(7);

            var restored = edited.WithHeroAsIn(36, original);

            Assert.That(restored.HeroIds, Is.EqualTo(Numbers.Of(115, 36, 55, 7)));
        }

        [Test]
        public void PuttingAHeroBackIntoAFullListIsRefused()
        {
            var original = Player(null, 1, 2, 3, 4, 5, 6, 7, 8);
            var full = original.WithoutHero(8).WithHero(9);

            Assert.That(() => full.WithHeroAsIn(8, original), Throws.TypeOf<GameRuleException>());
        }
    }
}
