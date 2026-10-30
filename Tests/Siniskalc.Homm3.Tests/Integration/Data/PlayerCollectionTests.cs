using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Runtime;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// Tests that the players and the document tell their colour and game version.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerCollectionTests
    {
        private static Game Load(params string[] path) => SavedGames.Get(Path.Combine(["TestData", "Hota", .. path]));

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
