namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests an entry of the list of heroes a place of the tavern can offer: a hero or nobody.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroChoiceViewModelTests
    {
        [Test]
        public void AChoiceOfAHeroKeepsItsIdAndNameAndUsesTheIdAsKey()
        {
            var choice = new HeroChoiceViewModel(12, "Orrin");

            Assert.That((choice.HeroId, choice.Key, choice.Name), Is.EqualTo(((int?)12, 12, "Orrin")));
        }

        [Test]
        public void TheChoiceOfNobodyHasNoIdAndTheKeyMinusOne()
        {
            var choice = new HeroChoiceViewModel(null, "(nobody)");

            Assert.That((choice.HeroId, choice.Key), Is.EqualTo(((int?)null, HeroChoiceViewModel.NoneKey)));
            Assert.That(HeroChoiceViewModel.NoneKey, Is.EqualTo(-1));
        }

        [Test]
        public void AHeroWithIdZeroIsNotNobody()
        {
            Assert.That(new HeroChoiceViewModel(0, "Orrin").Key, Is.Not.EqualTo(HeroChoiceViewModel.NoneKey));
        }

        [Test]
        public void AChoiceIsShownAsItsNameInAList()
        {
            Assert.That(new HeroChoiceViewModel(3, "Valeska").ToString(), Is.EqualTo("Valeska"));
        }
    }
}
