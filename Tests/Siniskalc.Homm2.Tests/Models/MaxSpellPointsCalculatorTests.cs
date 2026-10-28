namespace Siniskalc.Homm2.Tests.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The most spell points a HoMM2 hero can have.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MaxSpellPointsCalculatorTests
    {
        private readonly MaxSpellPointsCalculator calculator = new();

        [Test]
        public void AHeroCanHaveTenSpellPointsForEachPointOfKnowledge()
        {
            Assert.That(this.calculator.GetMaximum(new PrimarySkills(1, 1, 1, 7), SecondarySkills.None), Is.EqualTo(70));
        }

        [Test]
        public void AKnowledgeBelowZeroGivesNoSpellPoints()
        {
            Assert.That(this.calculator.GetMaximum(new PrimarySkills(1, 1, 1, -3), SecondarySkills.None), Is.EqualTo(0));
        }
    }
}
