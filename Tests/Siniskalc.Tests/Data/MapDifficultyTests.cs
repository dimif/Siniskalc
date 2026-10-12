using Siniskalc.Entities;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The map's difficulty is set when the map is created; it is not the difficulty the game was started at.
    /// </summary>
    [TestFixture]
    internal sealed class MapDifficultyTests
    {
        private static Game Load(string folder, string name) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", folder, name + ".GM1"));

        // castles_01_blacksmith_garrison_tumlac_to_blue is a map made Impossible; map_difficulty_edited_to_expert is the same save with the map edited to Expert.
        [TestCase("Hota", "castles_01_blacksmith_garrison_tumlac_to_blue", GameDifficulty.King)]
        [TestCase("Hota", "map_difficulty_edited_to_expert", GameDifficulty.Queen)]
        [TestCase("Hota", "empire_of_the_world_01_m5w4d6", GameDifficulty.Rook)]
        [TestCase("Hota", "bios_standard", GameDifficulty.Queen)]
        [TestCase("Sod", "sod_day_01_m1w1d1", GameDifficulty.King)]
        public void TheMapDifficultyIsRead(string folder, string save, GameDifficulty expected)
        {
            Assert.That(Load(folder, save).MapDifficulty, Is.EqualTo(expected));
        }

        [Test]
        public void TheMapDifficultyIsNotTheGameDifficulty()
        {
            var game = Load("Hota", "empire_of_the_world_01_m5w4d6");

            Assert.That(game.Difficulty, Is.EqualTo(GameDifficulty.Queen));
            Assert.That(game.MapDifficulty, Is.Not.EqualTo(game.Difficulty));
        }
    }
}
