namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// Tests the card of the adventure spells that are active on a HoMM2 hero.
    /// </summary>
    [TestFixture]
    internal sealed class HeroActiveSpellsTests
    {
        [Test]
        public void VisionsIsTheOnlySpellOfTheCardAndItsBoxIsKeptButNotSaved()
        {
            var game = TestServices.LoadBrokenAlliance(24);
            var hero = game.History.Current.Heroes().GetModels().First(h => h.Owner != null);
            var page = new HeroViewModel(game, hero.Id, _ => { }, TestServices.SpellPoints);

            var visions = page.ActiveSpells.Spells.Single();
            Assert.That(visions.Name, Is.EqualTo("Visions"));
            Assert.That(visions.IsChecked, Is.False);

            visions.IsChecked = true;

            Assert.That(visions.IsChecked, Is.True);
            Assert.That(game.History.Current, Is.SameAs(game.History.Original), "the savegame is not touched");
        }
    }
}
