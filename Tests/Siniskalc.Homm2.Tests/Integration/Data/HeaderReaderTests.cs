namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The difficulty of the game, the difficulty of the map and the size of the map, from the saves of TestData/Homm2/MapDifficultySize.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class HeaderReaderTests
    {
        private const string Series = Saves.MapDifficultySize;

        private static Homm2.Runtime.Game Load(string name) => TestServices.Load(Saves.Path(Series, "map_difficulty_size_" + name));

        [TestCase("01_game_easy", GameDifficulty.Pawn)]
        [TestCase("02_game_normal", GameDifficulty.Knight)]
        [TestCase("03_game_hard", GameDifficulty.Rook)]
        [TestCase("04_game_expert", GameDifficulty.Queen)]
        [TestCase("05_game_impossible", GameDifficulty.King)]
        [TestCase("06_map_easy_small", GameDifficulty.Knight)]
        [TestCase("07_map_hard_extra_large", GameDifficulty.Knight)]
        [TestCase("08_map_expert_large", GameDifficulty.Knight)]
        [TestCase("09_map_hard_medium", GameDifficulty.Knight)]
        public void TheDifficultyTheGameWasStartedAtIsRead(string save, GameDifficulty expected)
        {
            Assert.That(Load(save).Difficulty, Is.EqualTo(expected));
        }

        [TestCase("01_game_easy", GameDifficulty.Knight)]
        [TestCase("05_game_impossible", GameDifficulty.Knight)]
        [TestCase("06_map_easy_small", GameDifficulty.Pawn)]
        [TestCase("07_map_hard_extra_large", GameDifficulty.Rook)]
        [TestCase("08_map_expert_large", GameDifficulty.Queen)]
        [TestCase("09_map_hard_medium", GameDifficulty.Rook)]
        public void TheDifficultyTheMapWasMadeWithIsReadApartFromTheGameDifficulty(string save, GameDifficulty expected)
        {
            Assert.That(Load(save).Map!.Difficulty, Is.EqualTo(expected));
        }

        [TestCase("01_game_easy", 36, "Small (36 x 36)")]
        [TestCase("06_map_easy_small", 36, "Small (36 x 36)")]
        [TestCase("09_map_hard_medium", 72, "Medium (72 x 72)")]
        [TestCase("08_map_expert_large", 108, "Large (108 x 108)")]
        [TestCase("07_map_hard_extra_large", 144, "Extra large (144 x 144)")]
        public void TheSizeOfTheMapIsReadInSquaresAndByName(string save, int size, string text)
        {
            var map = Load(save).Map!;

            Assert.That(map.Size, Is.EqualTo(size));
            Assert.That(map.SizeText, Is.EqualTo(text));
        }

        [Test]
        public void TheDifficultyOfTheOlderSavesIsRead()
        {
            Assert.That(TestServices.Load(Saves.BrokenAllianceSave(1)).Difficulty, Is.EqualTo(GameDifficulty.Knight));
            Assert.That(TestServices.Load(Saves.Path(Saves.ArraxTheJerk, "arrax_the_jerk_01_start")).Difficulty, Is.EqualTo(GameDifficulty.King));
        }
    }
}
