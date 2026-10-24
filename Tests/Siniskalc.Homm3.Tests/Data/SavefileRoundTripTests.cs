namespace Siniskalc.Homm3.Tests.Data
{
    using Siniskalc.Homm3.Data;

    /// <summary>
    /// What reading a savefile and writing it back must leave alone, for each savefile of the series the repository holds.
    /// </summary>
    [TestFixture]
    internal sealed class SavefileRoundTripTests
    {
        private static IEnumerable<TestCaseData> Savefiles()
        {
            yield return Case("Sod", "sod_game_00_baseline");
            yield return Case("Sod", "sod_game_01_red_gives_blue_resources");
            yield return Case("Hota", "empire_of_the_world_01_m5w4d6");
            yield return Case("Hota", "castles_01_blacksmith_garrison_tumlac_to_blue");
            yield return Case(Path.Combine("Hota", "Arts"), "arts_06_arts");
            yield return Case(Path.Combine("Hota", "Arts"), "arts_07_before_adv_spells");
            yield return Case(Path.Combine("Hota", "Arts"), "arts_12_all");
        }

        [TestCaseSource(nameof(Savefiles))]
        public void EveryHeroOfASavefileSerializesToTheBytesItWasReadFrom(string path)
        {
            var heroes = TestServices.GameService.LoadGameFromFile(path).History.Current.Heroes();
            var entities = TestServices.VersionEntities.For(heroes.Offsets.GameVersion);
            var deserializer = new HeroDeserializer(heroes.Offsets, entities);
            var serializer = new SafeHeroSerializer(heroes.Offsets, entities);

            foreach (var chunk in heroes)
            {
                Assert.That(serializer.Serialize(chunk, deserializer.Deserialize(chunk)).HasSameBytes(chunk), Is.True, chunk.Name);
            }
        }

        [TestCaseSource(nameof(Savefiles))]
        public void AGameThatWasNotEditedSavesTheBytesItWasLoadedFrom(string path)
        {
            var game = TestServices.GameService.LoadGameFromFile(path);

            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
        }

        private static TestCaseData Case(string folder, string name) =>
            new TestCaseData(Path.Combine("TestData", folder, name + ".GM1")).SetName(name);
    }
}
