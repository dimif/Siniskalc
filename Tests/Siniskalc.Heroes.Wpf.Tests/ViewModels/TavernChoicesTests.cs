namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests who a place of a tavern can be given to.
    /// </summary>
    [TestFixture]
    internal sealed class TavernChoicesTests
    {
        private static readonly HeroSummary<ConsoleColor>[] Heroes =
        [
            new(0, 10, "Ann", null, "Knight", 1),
            new(1, 11, "Bob", null, "Knight", 1),
            new(2, 12, "Cid", ConsoleColor.Red, "Knight", 1),
            new(3, 13, "Bob", null, "Wizard", 1),
        ];

        [Test]
        public void ThePlaceOffersNobodyAndTheHeroesNoPlayerOwnsExceptTheOneInTheOtherPlace()
        {
            var choices = TavernChoices.For(new TavernHeroes(null, 10), 0, Heroes);

            Assert.That(choices.Select(c => c.HeroId), Is.EqualTo(new int?[] { null, 11, 13 }));
        }

        [Test]
        public void ANameThatTwoHeroesShareIsToldApartByTheNumber()
        {
            var choices = TavernChoices.For(TavernHeroes.Empty, 0, Heroes);

            Assert.That(choices.Select(c => c.Name), Is.EqualTo(new[] { "(none)", "Ann", "Bob (id 11)", "Bob (id 13)" }));
        }

        [Test]
        public void AHeroThatIsInThePlaceStaysInTheChoicesWhoeverOwnsHim()
        {
            var choices = TavernChoices.For(new TavernHeroes(12, null), 0, Heroes);

            Assert.That(choices.Select(c => c.HeroId), Does.Contain(12));
        }

        [Test]
        public void ANumberWithNoHeroInTheSavefileIsKeptAsUnknown()
        {
            var choices = TavernChoices.For(new TavernHeroes(99, null), 0, Heroes);

            Assert.That(choices.Last().Name, Is.EqualTo("Unknown hero (id 99)"));
        }
    }
}
