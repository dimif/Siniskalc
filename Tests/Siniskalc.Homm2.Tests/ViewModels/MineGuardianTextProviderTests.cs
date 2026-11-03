namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The wording of the read-only mine guardian display.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MineGuardianTextProviderTests
    {
        [Test]
        public void AnAbsentGuardianIsDescribedAsUnguarded()
        {
            Assert.That(new MineGuardianTextProvider().Describe(null), Is.EqualTo("Unguarded"));
        }

        [Test]
        public void AKnownGuardianShowsTheCatalogCreatureNameAndTheSavedCount()
        {
            var guardian = new MineGuardian(62, 40, TestCatalog.Standard.Creatures["EarthElemental"]);

            Assert.That(new MineGuardianTextProvider().Describe(guardian), Is.EqualTo("Earth Elemental × 40"));
        }

        [Test]
        public void AnUnknownGuardianSaysWhichSpellAndCountWereSaved()
        {
            var guardian = new MineGuardian(91, 17, null);

            Assert.That(new MineGuardianTextProvider().Describe(guardian), Is.EqualTo("Unknown guardian (spell 91, count 17)"));
        }
    }
}
