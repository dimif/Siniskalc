namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Tests the spell row of the list of spells of a hero of HoMM2.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SpellRowViewModelTests
    {
        [Test]
        public void ASpellRowShowsTheSpellItsLevelItsTypeItsMagicAndItsCost()
        {
            var row = new SpellRowViewModel(TestCatalog.Standard.Spells["Fireball"], (_, _) => { });

            Assert.That((row.Name, row.Level, row.Kind, row.Magic, row.Cost, row.Spell), Is.EqualTo(("Fireball", 3, "Combat", "Elemental", 9, TestCatalog.Standard.Spells["Fireball"])));
            Assert.That(row.Group, Is.EqualTo("Elemental"));
        }

        [Test]
        public void ASpellOfNoMagicHasNoMagicText()
        {
            Assert.That(new SpellRowViewModel(TestCatalog.Standard.Spells["Cure"], (_, _) => { }).Magic, Is.Empty);
        }

        [Test]
        public void TickingTheRowTellsTheCallerWhichRowAndTheNewValue()
        {
            var seen = new List<(string Name, bool Learned)>();
            var row = new SpellRowViewModel(TestCatalog.Standard.Spells["Cure"], (r, learned) => seen.Add((r.Name, learned)));

            row.IsLearned = true;

            Assert.That(seen, Is.EqualTo(new[] { ("Cure", true) }));
        }
    }
}
