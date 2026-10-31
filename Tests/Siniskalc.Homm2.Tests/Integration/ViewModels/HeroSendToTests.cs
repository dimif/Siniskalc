namespace Siniskalc.Homm2.Tests.Integration.ViewModels
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The send to command of the position card of a HoMM2 hero: the castles and the mines of its player, a castle with the tile under its own tile as the
    /// entrance.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroSendToTests
    {
        [Test]
        public void TheHeroIsSentToTheTileUnderACastleOrOnAMineOfItsPlayerAndANeutralHeroHasNothingToGoTo()
        {
            var game = TestServices.LoadBrokenAlliance(24);
            var document = game.History.Current;
            var castles = document.Castles();
            var hero = document.Heroes().GetModels().First(h => !h.Owner.IsNeutral && castles.UsedIndexes().Any(i => castles.GetModel(i).Owner == h.Owner));
            var page = new HeroViewModel(game, hero.Id, _ => { }, TestServices.SpellPoints);
            var send = page.Position.SendTo!;
            var target = send.Destinations.First(c => c.Kind == "Castle" && !c.IsOccupied);
            var castle = castles.UsedIndexes().Select(castles.GetModel).Single(c => c.Name == target.Name);

            Assert.That((target.Entrance.X, target.Entrance.Y), Is.EqualTo((castle.Position.X, castle.Position.Y + 1)));
            Assert.That(send.Destinations.Where(c => c.Kind == "Castle").Select(c => c.Name), Is.EquivalentTo(castles.UsedIndexes().Select(castles.GetModel).Where(c => c.Owner == hero.Owner).Select(c => c.Name)));
            var mines = document.Mines();
            var ownedMines = Enumerable.Range(0, mines.Count).Select(mines.GetModel).Where(m => m.Owner == hero.Owner).ToList();
            var listedMines = send.Destinations.Where(c => c.Kind == "Mine").ToList();
            Assert.That(listedMines, Has.Count.EqualTo(ownedMines.Count));
            Assert.That(listedMines.Select(m => (m.Entrance.X, m.Entrance.Y)), Is.EquivalentTo(ownedMines.Select(m => (m.Position.X, m.Position.Y))));
            Assert.That(listedMines.All(m => m.IsApproximate), Is.True);

            send.SendCommand.Execute(target);

            Assert.That((page.Position.X, page.Position.Y), Is.EqualTo((target.Entrance.X, target.Entrance.Y)));
            Assert.That(page.HasPendingChanges, Is.True);

            var neutral = document.Heroes().GetModels().First(h => h.Owner.IsNeutral);
            Assert.That(new HeroViewModel(game, neutral.Id, _ => { }, TestServices.SpellPoints).Position.SendTo!.HasDestinations, Is.False);
        }
    }
}
