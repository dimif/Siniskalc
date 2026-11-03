using Siniskalc.Core.Data;
using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// The session of a player: only its model counts as its changes, so the list of heroes it keeps, which follows the owners of the heroes,
    /// neither makes it dirty nor is put back with it. Replayed on the baseline save of <c>TestData/Hota/HeroOwners</c>.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerEditSessionTests
    {
        private const int Clancy = 22;

        private static SaveDocumentHistory Load() =>
            SavedGames.Get(Path.Combine("TestData", "Hota", "HeroOwners", "hero_owners_00_baseline.GM2")).History;

        private static PlayerLists Lists(SaveDocument document, PlayerColor color) => document.Players().GetLists(document.Players().FindByColor(color));

        private static HeroModel Hero(SaveDocument document, int heroId) => document.Heroes().GetModel(document.Heroes().IndexOfId(heroId));

        private static PlayerEditSession SessionOf(SaveDocumentHistory history, PlayerColor color) =>
            history.CreatePlayerSession(history.Current.Players().FindByColor(color));

        private static void GiveClancyTo(SaveDocumentHistory history, PlayerColor owner) =>
            history.CreateHeroSession(history.Current.Heroes().IndexOfId(Clancy)).Update(h => h.WithOwner(owner));

        [Test]
        public void ThePlayerSessionIsOfThePlayerItWasOpenedOn()
        {
            var history = Load();
            var index = history.Current.Players().Count - 1;

            var session = history.CreatePlayerSession(index);

            Assert.That(session.PlayerIndex, Is.EqualTo(index));
            Assert.That(session.Current, Is.EqualTo(history.Current.Players().GetModel(index)));
        }

        [Test]
        public void GivingAHeroAwayLeavesThePlayersWhoseListsChangedWithoutChangesOfTheirOwn()
        {
            var history = Load();
            var owner = Hero(history.Current, Clancy).Owner;

            GiveClancyTo(history, PlayerColor.Tan);

            Assert.That(Lists(history.Current, owner).HeroIds, Does.Not.Contain(Clancy), "the list changed");
            Assert.That(SessionOf(history, owner).IsDirty, Is.False);
            Assert.That(SessionOf(history, PlayerColor.Tan).IsDirty, Is.False);
        }

        [Test]
        public void AnEditOfThePlayerMakesItDirty()
        {
            var history = Load();
            var session = SessionOf(history, PlayerColor.Blue);

            session.Update(p => p with { Resources = p.Resources.With(Resource.Gold, p.Resources[Resource.Gold] + 1) });

            Assert.That(session.IsDirty, Is.True);
        }

        [Test]
        public void PuttingThePlayerBackLeavesTheHeroesItLostOrGainedWithTheirOwners()
        {
            var history = Load();
            var owner = Hero(history.Current, Clancy).Owner;
            var session = SessionOf(history, owner);
            session.Update(p => p with { Resources = p.Resources.With(Resource.Gold, 1) });
            GiveClancyTo(history, PlayerColor.Neutral);
            var lists = Lists(history.Current, owner);

            session.Update(_ => session.Original);

            Assert.That(session.IsDirty, Is.False);
            Assert.That(Hero(history.Current, Clancy).Owner, Is.EqualTo(PlayerColor.Neutral));
            Assert.That(Lists(history.Current, owner), Is.EqualTo(lists), "the list still follows the owner of Clancy");
        }
    }
}
