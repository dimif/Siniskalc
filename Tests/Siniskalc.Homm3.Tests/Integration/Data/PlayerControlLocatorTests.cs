using Siniskalc;
using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// Tests against HotA savegames of one map (red is Rampart, tan is Tower) where either red or tan was the human.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerControlLocatorTests
    {
        private static Game Load(string name) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", name + (name == "players_human_red_and_tan" ? ".GM2" : ".GM1")));

        [Test]
        public void RedIsTheHumanInHumanRed()
        {
            var game = Load("players_human_red");

            Assert.That(game.GetControl(PlayerColor.Red), Is.EqualTo(PlayerType.Human));
            Assert.That(game.GetControl(PlayerColor.Blue), Is.EqualTo(PlayerType.Computer));
            Assert.That(game.GetControl(PlayerColor.Tan), Is.EqualTo(PlayerType.Computer));
            Assert.That(game.GetControl(PlayerColor.Green), Is.EqualTo(PlayerType.Computer));
            Assert.That(game.GetControl(PlayerColor.Orange), Is.EqualTo(PlayerType.NotInGame));
        }

        [Test]
        public void RedAndTanAreHumansInTheHotseatSavegame()
        {
            var game = Load("players_human_red_and_tan");

            Assert.That(game.GetControl(PlayerColor.Red), Is.EqualTo(PlayerType.Human));
            Assert.That(game.GetControl(PlayerColor.Blue), Is.EqualTo(PlayerType.Computer));
            Assert.That(game.GetControl(PlayerColor.Tan), Is.EqualTo(PlayerType.Human));
            Assert.That(game.GetControl(PlayerColor.Green), Is.EqualTo(PlayerType.Computer));
            Assert.That(game.GetControl(PlayerColor.Orange), Is.EqualTo(PlayerType.NotInGame));
        }

        [Test]
        public void TanIsTheHumanInHumanTan()
        {
            var game = Load("players_human_tan");

            Assert.That(game.GetControl(PlayerColor.Red), Is.EqualTo(PlayerType.Computer));
            Assert.That(game.GetControl(PlayerColor.Tan), Is.EqualTo(PlayerType.Human));
        }

        [TestCase("empire_of_the_world_01_m5w4d6", "Purple")]
        [TestCase("bios_standard", "Red")]
        public void TheHumanOfOtherSavegamesIsRead(string save, string humanName)
        {
            var human = PlayerColor.All.Single(c => c.Name == humanName);
            var game = Load(save);

            Assert.That((game.PlayerControls ?? []).Count(c => c == PlayerType.Human), Is.EqualTo(1));
            Assert.That(game.GetControl(human), Is.EqualTo(PlayerType.Human));
        }

        [TestCase("empire_of_the_world_01_m5w4d6")]
        [TestCase("players_human_red")]
        [TestCase("players_human_tan")]
        [TestCase("bios_standard")]
        public void TheDifficultyIsRead(string save)
        {
            // empire_of_the_world_01_m5w4d6 is shown as Queen by the game; the others were all started at the same level.
            Assert.That(Load(save).Difficulty, Is.EqualTo(GameDifficulty.Queen));
        }

        [TestCase("players_human_red", "Red")]
        [TestCase("players_human_tan", "Tan")]
        [TestCase("players_human_red_and_tan", "Red")]
        [TestCase("empire_of_the_world_01_m5w4d6", "Purple")]
        public void TheColourWhoseTurnItIsIsRead(string save, string currentName)
        {
            var current = PlayerColor.All.Single(c => c.Name == currentName);
            Assert.That(Load(save).CurrentPlayer, Is.EqualTo(current));
        }

        [Test]
        public void UnknownVersionsAndBytesWithoutPlayersGiveNone()
        {
            Assert.That(new PlayerControlLocator().Find(new byte[5000], GameVersion.HornOfTheAbyss), Is.Null);
            Assert.That(new PlayerControlLocator().Find(new byte[5000], GameVersion.Unknown), Is.Null);
        }
    }
}
