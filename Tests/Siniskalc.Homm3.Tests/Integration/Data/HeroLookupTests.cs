using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Runtime;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// Tests that the collections answer the questions view models ask of heroes, so that the view models never read chunks.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeroLookupTests
    {
        private static Game Load(params string[] path) => SavedGames.Get(Path.Combine(["TestData", "Hota", .. path]));

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
            Assert.That(game.History.CreateHeroSession(ufretin!.Index).Current.Id, Is.EqualTo(17));
            Assert.That(heroes.GetSummary(orrin!.Index), Is.EqualTo(orrin));
        }
    }
}
