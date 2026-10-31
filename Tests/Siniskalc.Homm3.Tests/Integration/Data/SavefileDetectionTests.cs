namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Data;

    /// <summary>
    /// A savefile is recognised as the game it was saved by and holds all the heroes of that game.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class SavefileDetectionTests
    {
        private const int ShadowOfDeathHeroes = 156;

        private static IEnumerable<TestCaseData> Savefiles()
        {
            yield return new TestCaseData(Path.Combine("TestData", "Sod", "sod_game_00_baseline.GM1"), GameVersion.ShadowOfDeath, ShadowOfDeathHeroes).SetName("sod_game_00_baseline");
            yield return new TestCaseData(Path.Combine("TestData", "Sod", "sod_game_01_red_gives_blue_resources.GM1"), GameVersion.ShadowOfDeath, ShadowOfDeathHeroes).SetName("sod_game_01_red_gives_blue_resources");
            yield return new TestCaseData(Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1"), GameVersion.HornOfTheAbyss, 215).SetName("empire_of_the_world_01_m5w4d6");

            foreach (var path in Directory.GetFiles(Path.Combine("TestData", "Sod", "Hotseat"), "*.GM2").OrderBy(f => f, StringComparer.Ordinal))
            {
                yield return new TestCaseData(path, GameVersion.ShadowOfDeath, ShadowOfDeathHeroes).SetName(Path.GetFileNameWithoutExtension(path));
            }
        }

        [TestCaseSource(nameof(Savefiles))]
        public void ASavefileIsDetectedAsItsGameWithAllItsHeroes(string path, GameVersion version, int heroes)
        {
            var game = TestServices.GameService.LoadGameFromFile(path);

            Assert.That(game.GameVersion, Is.EqualTo(version));
            Assert.That(game.History.Current.Heroes(), Has.Count.EqualTo(heroes));
        }
    }
}
