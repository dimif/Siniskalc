namespace Siniskalc.Homm2.Tests.Models
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The most spell points a HoMM2 hero can have.
    /// </summary>
    [TestFixture]
    internal sealed class SpellPointsCalculatorTests
    {
        [Test]
        public void AHeroCanHaveTenSpellPointsForEachPointOfKnowledge()
        {
            var game = TestServices.LoadBrokenAlliance(24);
            var hero = game.History.Current.Heroes().GetModels().First();

            var most = new MaxSpellPointsCalculator().GetMaximum(hero with { PrimarySkills = hero.PrimarySkills with { Knowledge = 7 } });

            Assert.That(most, Is.EqualTo(70));
        }
    }
}
