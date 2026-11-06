using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Runtime;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// Tests that the collections answer the questions view models ask of castles, so that the view models never read chunks.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CastleLookupTests
    {
        private static Game Load(params string[] path) => SavedGames.Get(Path.Combine(["TestData", "Hota", .. path]));

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
    }
}
