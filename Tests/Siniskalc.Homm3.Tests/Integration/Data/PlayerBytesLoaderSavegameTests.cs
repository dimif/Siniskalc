using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Runtime;

using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>
    /// How the table of players is found in a savefile (the saves in TestData/Hota/Banishing).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class PlayerBytesLoaderSavegameTests
    {
        private static Game Load(string save) =>
            SavedGames.Get(Directory.GetFiles(Path.Combine("TestData", "Hota", "Banishing"), save + ".GM?").Single());

        [TestCase("banishing_00_baseline")]
        [TestCase("banishing_14_ban_count_start")]
        [TestCase("banishing_15_m4w1d4")]
        [TestCase("banishing_16_m4w2d3")]
        public void RedAndBlueAreFoundWhateverTheFlagByteOfTheirStructIs(string save)
        {
            var colors = Load(save).History.Current.Players().Select(c => c.Color).ToList();

            Assert.That(colors, Does.Contain(PlayerColor.Red));
            Assert.That(colors, Does.Contain(PlayerColor.Blue));
        }

        [TestCase("banishing_14_ban_count_start", 7)]
        [TestCase("banishing_15_m4w1d4", 6)]
        [TestCase("banishing_16_m4w2d3", 0)]
        public void TheDaysBlueHasLeftAreReadInTheSavesOfTheFourPlayerMap(string save, int daysLeft)
        {
            var players = Load(save).History.Current.Players();

            Assert.That(TestServices.PlayerDeserializer.Deserialize(players[players.FindByColor(PlayerColor.Blue)]).DaysLeft, Is.EqualTo(daysLeft));
        }
    }
}
