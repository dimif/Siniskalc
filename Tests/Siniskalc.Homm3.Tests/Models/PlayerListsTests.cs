using Siniskalc.Core.Models;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Models
{
    /// <summary>
    /// The reducer of the list of heroes of a player, as the game changes it in the saves of <c>TestData/Hota/HeroOwners</c>.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerListsTests
    {
        private static PlayerLists Lists(int? selected, params int[] heroIds) => new(new Numbers(heroIds), selected);

        [Test]
        public void LosingAHeroMovesTheHeroesAfterItUpAPlace()
        {
            var original = Lists(null, 115, 36, 55, 84, 102, 17, 144);

            var red = original.WithHero(17, false, original);

            Assert.That(red.HeroIds, Is.EqualTo(Numbers.Of(115, 36, 55, 84, 102, 144)));
            Assert.That(red.SelectedHero, Is.Null);
        }

        [Test]
        public void LosingTheSelectedHeroSelectsTheFirstHeroLeft()
        {
            var original = Lists(22, 0, 22);

            var blue = original.WithHero(22, false, original);

            Assert.That(blue.HeroIds, Is.EqualTo(Numbers.Of(0)));
            Assert.That(blue.SelectedHero, Is.EqualTo(0));
        }

        [Test]
        public void LosingTheLastHeroLeavesNoHeroSelected()
        {
            var original = Lists(22, 22);

            var blue = original.WithHero(22, false, original);

            Assert.That(blue.HeroIds, Is.Empty);
            Assert.That(blue.SelectedHero, Is.Null);
        }

        [Test]
        public void ListsThatAlreadySaySoAreReturnedAsTheyAre()
        {
            var blue = Lists(0, 0, 22);

            Assert.That(blue.WithHero(29, false, blue), Is.SameAs(blue));
            Assert.That(blue.WithHero(22, true, blue), Is.SameAs(blue));
        }

        [Test]
        public void GainingANewHeroPutsItAtTheEndOfTheListAndKeepsTheSelectedHero()
        {
            var original = Lists(null, 115, 36);

            var red = original.WithHero(65, true, original);

            Assert.That(red.HeroIds, Is.EqualTo(Numbers.Of(115, 36, 65)));
            Assert.That(red.SelectedHero, Is.Null);
        }

        [Test]
        public void AListMayComeOutLongerThanTheGameAllowsForTheCallerToRefuse()
        {
            var blue = Lists(null, 0, 169, 164, 184, 129, 70, 71, 10);

            Assert.That(blue.WithHero(29, true, blue).HeroIds.Count, Is.EqualTo(9));
        }

        [Test]
        public void ALostHeroThatComesBackReturnsToItsPlaceAndItsSelection()
        {
            var original = Lists(22, 0, 22);

            Assert.That(original.WithHero(22, false, original).WithHero(22, true, original), Is.EqualTo(original));
        }

        [Test]
        public void AGainedHeroThatLeavesAgainGivesTheListOfTheSavegameBack()
        {
            var original = Lists(null, 115, 36);

            Assert.That(original.WithHero(65, true, original).WithHero(65, false, original), Is.EqualTo(original));
        }

        [Test]
        public void OneHeroThatComesBackLeavesTheOtherChangesOfTheList()
        {
            var original = Lists(null, 115, 36, 55, 84);
            var edited = original.WithHero(36, false, original).WithHero(84, false, original).WithHero(7, true, original);

            var restored = edited.WithHero(36, true, original);

            Assert.That(restored.HeroIds, Is.EqualTo(Numbers.Of(115, 36, 55, 7)));
        }

        [Test]
        public void ASelectionTheSavegameHadComesBackOnlyWithTheWholeList()
        {
            var original = Lists(36, 115, 36, 55);
            var edited = original.WithHero(36, false, original).WithHero(55, false, original);

            var partly = edited.WithHero(36, true, original);

            Assert.That((partly.HeroIds, partly.SelectedHero), Is.EqualTo((Numbers.Of(115, 36), (int?)115)));
            Assert.That(partly.WithHero(55, true, original), Is.EqualTo(original));
        }
    }
}
