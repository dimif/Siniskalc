using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// TestData/Hota/tan_not_parsing: a Horn of the Abyss map with a town in each corner, played by Red, Blue, Green and Teal. The colour Tan is not in
    /// the game (its control byte says so) but has a table entry with 10000 gold and nothing else. The town Port Evendore (Cove) and the hero Spint
    /// (the Navigator) of the bottom left corner belong to the colour Teal in the savefile.
    /// </summary>
    [TestFixture]
    internal sealed class HotaFourCornersTests
    {
        private static Game Load() => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "tan_not_parsing.GM1"));

        [Test]
        public void ThePlayersAreTheFourCornersAndTheColourThatIsNotInTheGameIsNotListed()
        {
            var game = Load();

            Assert.That(game.PlayerControls![(int)PlayerColor.Tan], Is.EqualTo(PlayerType.NotInGame));
            Assert.That(game.History.Current.Players().GetColors(), Is.EqualTo(new[] { PlayerColor.Red, PlayerColor.Blue, PlayerColor.Green, PlayerColor.Teal }));
        }

        [Test]
        public void PortEvendoreAndSpintBelongToTheColourTheSavefileNamesForThem()
        {
            var game = Load();
            var castles = game.History.Current.Castles();
            var heroes = game.History.Current.Heroes();

            Assert.That(castles.GetModel(castles.FindByName("Port Evendore")).Owner, Is.EqualTo(PlayerColor.Teal));
            Assert.That(castles.GetModel(castles.FindByName("Port Evendore")).Faction!.Name, Is.EqualTo("Cove"));
            Assert.That(game.History.CreateHeroSession(heroes.FindByName("Spint")).Current.Owner, Is.EqualTo(PlayerColor.Teal));
        }
    }
}
