namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The next battle card of a HoMM2 hero: the places that gave it a bonus are counted, and one button takes everything away.
    /// </summary>
    [TestFixture]
    internal sealed class HeroNextBattleTests
    {
        [Test]
        public void TheCardCountsThePlacesAndAllNoneTakesTheBonusAndThePlacesAway()
        {
            var game = TestServices.LoadBrokenAlliance(24);
            var hero = game.History.Current.Heroes().GetModels().First(h => !h.Owner.IsNeutral);
            var page = new HeroViewModel(game, hero.Id, _ => { }, TestServices.SpellPoints);
            var card = page.NextBattle;
            var temple = card.Sources.Single(s => s.Name == "Temple");
            var fountain = card.Sources.Single(s => s.Name == "Fountain");
            temple.IsChecked = false;
            fountain.IsChecked = false;
            card.ClearCommand.Execute(null);

            temple.IsChecked = true;
            fountain.IsChecked = true;
            Assert.That(card.SourcesText, Is.EqualTo("2 of 5"));
            Assert.That(card.Bonus.Select(b => b.Value), Is.EqualTo(new long[] { 2, 1 }));
            Assert.That(card.ClearCommand.CanExecute(null), Is.True);

            card.ClearCommand.Execute(null);

            Assert.That(card.SourcesText, Is.EqualTo("0 of 5"));
            Assert.That(card.Bonus.Select(b => b.Value), Is.EqualTo(new long[] { 0, 0 }));
            Assert.That(card.ClearCommand.CanExecute(null), Is.False, "nothing is left to take away");
        }
    }
}
