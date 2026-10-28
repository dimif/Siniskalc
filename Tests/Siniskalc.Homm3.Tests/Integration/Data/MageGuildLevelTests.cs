namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Models;
    /// <summary>
    /// The mage guild level of a castle is the highest mage guild it has built, whatever the faction and the series of savefiles.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MageGuildLevelTests
    {
        private static readonly string[] RoeRedCastles = ["1_Castle", "2_Rampart", "3_Tower", "4_Inferno", "5_necropolis", "6_Dung", "7_Strong", "8_Fort"];

        private static readonly string[] OtherFactionsCastles = ["Corona", "Enkindle", "Grave Raven", "Castigare", "Kruber", "Deadfall", "Wazzar", "Highcastle"];

        private static IEnumerable<TestCaseData> Saves()
        {
            yield return Case(Path.Combine("Roe", "roe_09_town_hall"), RoeRedCastles, 0);
            yield return Case(Path.Combine("Roe", "roe_10_mage_guild_1"), RoeRedCastles, 1);
            yield return Case(Path.Combine("Roe", "roe_11_city_hall"), RoeRedCastles, 1);
            yield return Case(Path.Combine("Hota", "OtherFactions", "other_factions_10_mage_guild_1"), OtherFactionsCastles, 1);
            yield return Case(Path.Combine("Hota", "OtherFactions", "other_factions_11_mage_guild_2"), OtherFactionsCastles, 2);
            yield return Case(Path.Combine("Hota", "OtherFactions", "other_factions_12_mage_guild_3"), OtherFactionsCastles, 3);
            yield return Case(Path.Combine("Hota", "OtherFactions", "other_factions_42_city_hall"), OtherFactionsCastles, 3);
            yield return Case(Path.Combine("Sod", "conflux_08_resource_silo"), ["Conflux"], 0);
            yield return Case(Path.Combine("Sod", "conflux_09_mage_guild_1"), ["Conflux"], 1);
            yield return Case(Path.Combine("Sod", "conflux_13_mage_guild_5"), ["Conflux"], 5);
            yield return Case(Path.Combine("Hota", "Factory", "factory_14_mage_guild"), ["Fac Castle G"], 1);
            yield return Case(Path.Combine("Hota", "Factory", "factory_29_mage_guild_2"), ["Fac Castle G"], 2);
        }

        [TestCaseSource(nameof(Saves))]
        public void TheMageGuildLevelFollowsTheMageGuildBuilt(string path, string[] castleNames, int level)
        {
            var castles = TestServices.GameService.LoadGameFromFile(path).History.Current.Castles();

            foreach (var name in castleNames)
            {
                Assert.That(TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName(name)]).MageGuildLevel, Is.EqualTo(level), name);
            }
        }

        private static TestCaseData Case(string folderAndName, string[] castleNames, int level) =>
            new TestCaseData(Path.Combine("TestData", folderAndName + ".GM1"), castleNames, level).SetName(Path.GetFileName(folderAndName));
    }
}
