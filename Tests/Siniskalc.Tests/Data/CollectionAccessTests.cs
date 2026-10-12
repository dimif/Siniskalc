using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The collections answer the questions view models ask (a hero by id, the castle that holds a hero, every castle as a model),
    /// so that the view models never read chunks.
    /// </summary>
    [TestFixture]
    internal sealed class CollectionAccessTests
    {
        private static Game Load(params string[] path) => TestServices.GameService.LoadGameFromFile(Path.Combine(["TestData", "Hota", .. path]));

        [Test]
        public void HeroesAreFoundByTheIdCastlesHoldAndTheIdIsInTheModel()
        {
            var game = Load("Cove", "cove_coastal_06_garrisoned_hero.GM1");
            var heroes = game.History.Current.Heroes();

            var ufretin = heroes.FindById(17);
            var orrin = heroes.FindById(0);

            Assert.That(ufretin?.Name, Is.EqualTo("Ufretin"));
            Assert.That(orrin?.Name, Is.EqualTo("Orrin"));
            Assert.That(heroes.FindById(1000), Is.Null);
            Assert.That(heroes.GetSummaries().Select(s => s.HeroId), Is.Unique);
            Assert.That(game.History.CreateHeroSession(ufretin!.Index).Current.HeroId, Is.EqualTo(17));
            Assert.That(heroes.GetSummary(orrin!.Index), Is.EqualTo(orrin));
        }

        [Test]
        public void TheCastleThatHoldsAHeroIsFoundByTheHeroId()
        {
            var game = Load("Cove", "cove_coastal_06_garrisoned_hero.GM1");
            var castles = game.History.Current.Castles();
            var models = castles.GetModels();

            var garrisoned = models.Select((model, index) => (Model: model, Index: index)).Where(c => c.Model.GarrisonedHeroId != null).ToList();
            Assert.That(garrisoned, Is.Not.Empty);
            foreach (var (model, index) in garrisoned)
            {
                Assert.That(castles.FindByGarrisonedHero(model.GarrisonedHeroId!.Value), Is.EqualTo(index));
                Assert.That(game.History.Current.Heroes().FindById(model.GarrisonedHeroId.Value), Is.Not.Null);
                Assert.That(castles.GetName(index), Is.EqualTo(model.Name));
            }

            Assert.That(castles.FindByGarrisonedHero(1000), Is.EqualTo(-1));
        }

        [Test]
        public void TheCastleModelsAreReadOncePerVersionOfTheCollection()
        {
            var game = Load("Cove", "cove_coastal_06_garrisoned_hero.GM1");
            var castles = game.History.Current.Castles();

            var models = castles.GetModels();

            Assert.That(castles.GetModels(), Is.SameAs(models));
            Assert.That(models, Has.Count.EqualTo(castles.Count));
            Assert.That(castles.GetModel(0).Name, Is.EqualTo(castles.GetModels()[0].Name));
        }

        [Test]
        public void ASessionKnowsWhetherItIsEditedAndItsRevisionChangesWithEveryEdit()
        {
            var game = Load("empire_of_the_world_01_m5w4d6.GM1");
            var session = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Christian"));
            var other = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Maximus"));
            var revision = session.Revision;

            Assert.That(session.IsDirty, Is.False);

            other.Update(h => h with { SpellPoints = (ushort)(h.SpellPoints + 1) });
            Assert.That(session.Revision, Is.SameAs(revision), "another hero's edit leaves the revision alone");

            session.Update(h => h with { SpellPoints = (ushort)(h.SpellPoints + 1) });
            Assert.That(session.IsDirty, Is.True);
            Assert.That(session.Revision, Is.Not.SameAs(revision));

            game.History.Undo();
            Assert.That(session.IsDirty, Is.False);
        }

        [Test]
        public void ThePlayersAndTheDocumentTellTheirColourAndGameVersion()
        {
            var game = Load("empire_of_the_world_01_m5w4d6.GM1");
            var players = game.History.Current.Players();

            Assert.That(game.History.Current.GameVersion(), Is.EqualTo(GameVersion.HornOfTheAbyss));
            Assert.That(players.GetColor(0), Is.EqualTo(players.GetColors()[0]));
            Assert.That(players.GetModel(0), Is.Not.Null);
            Assert.That(game.History.Original.Players().GetModel(0).Resources, Is.EqualTo(players.GetModel(0).Resources));
        }
    }
}
