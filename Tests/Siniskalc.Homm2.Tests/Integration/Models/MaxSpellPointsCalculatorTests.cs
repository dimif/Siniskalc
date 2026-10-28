namespace Siniskalc.Homm2.Tests.Integration.Models
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The most spell points a HoMM2 hero can have.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MaxSpellPointsCalculatorTests
    {
        [Test]
        public void AHeroCanHaveTenSpellPointsForEachPointOfKnowledge()
        {
            var game = TestServices.LoadBrokenAlliance(24);
            var hero = game.History.Current.Heroes().GetModels().First();

            var most = new MaxSpellPointsCalculator().GetMaximum(hero.PrimarySkills with { Knowledge = 7 }, hero.SecondarySkills);

            Assert.That(most, Is.EqualTo(70));
        }
    }
}
