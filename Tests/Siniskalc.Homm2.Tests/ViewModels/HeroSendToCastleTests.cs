namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.ViewModels;

    /// <summary>
    /// The send to castle command of the position card of a HoMM2 hero: the castles of its player, each with the tile under its own tile as the
    /// entrance.
    /// </summary>
    [TestFixture]
    internal sealed class HeroSendToCastleTests
    {
        [Test]
        public void TheHeroIsSentToTheTileUnderACastleOfItsPlayerAndANeutralHeroHasNoCastles()
        {
            var game = TestServices.LoadBrokenAlliance(24);
            var document = game.History.Current;
            var castles = document.Castles();
            var hero = document.Heroes().GetModels().First(h => !h.Owner.IsNeutral && castles.UsedIndexes().Any(i => castles.GetModel(i).Owner == h.Owner));
            var page = new HeroViewModel(game, hero.Id, _ => { }, TestServices.SpellPoints);
            var send = page.Position.SendToCastle!;
            var target = send.Castles.First(c => !c.IsOccupied);
            var castle = castles.UsedIndexes().Select(castles.GetModel).Single(c => c.Name == target.Name);

            Assert.That((target.Entrance.X, target.Entrance.Y), Is.EqualTo((castle.Position.X, castle.Position.Y + 1)));
            Assert.That(send.Castles.Select(c => c.Name), Is.EquivalentTo(castles.UsedIndexes().Select(castles.GetModel).Where(c => c.Owner == hero.Owner).Select(c => c.Name)));

            send.SendCommand.Execute(target);

            Assert.That((page.Position.X, page.Position.Y), Is.EqualTo((target.Entrance.X, target.Entrance.Y)));
            Assert.That(page.HasPendingChanges, Is.True);

            var neutral = document.Heroes().GetModels().First(h => h.Owner.IsNeutral);
            Assert.That(new HeroViewModel(game, neutral.Id, _ => { }, TestServices.SpellPoints).Position.SendToCastle!.HasCastles, Is.False);
        }
    }
}
