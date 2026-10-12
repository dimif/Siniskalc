using Siniskalc;
using Siniskalc.Entities;
using Siniskalc.Data;
using Siniskalc.Models;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// Tests against real HotA savegames of one game, made on consecutive days. The dates come from the in-game adventure map.
    /// </summary>
    [TestFixture]
    internal sealed class GameDateTests
    {
        private static Game Load(string name)
        {
            var path = Path.Combine("TestData", name.StartsWith("sod") ? "Sod" : "Hota", name + ".GM1");
            return TestServices.GameService.LoadGameFromFile(path);
        }

        [TestCase("empire_of_the_world_01_m5w4d6", 5, 4, 6)]
        [TestCase("empire_of_the_world_02_m5w4d6_purple_gifts_red", 5, 4, 6)]
        [TestCase("new_day_01_m5w4d7", 5, 4, 7)]
        [TestCase("new_day_02_m6w1d1", 6, 1, 1)]
        [TestCase("new_day_03_m6w1d2", 6, 1, 2)]
        public void TheDateOfEachSavegameIsRead(string save, int month, int week, int day)
        {
            Assert.That(Load(save).Date, Is.EqualTo(new GameDate(month, week, day)));
        }

        [TestCase("sod_day_01_m1w1d1", 1, 1, 1)]
        [TestCase("sod_day_02_m1w1d2", 1, 1, 2)]
        [TestCase("sod_day_03_m1w1d3", 1, 1, 3)]
        public void TheDateOfShadowOfDeathSavegamesIsRead(string save, int month, int week, int day)
        {
            var game = Load(save);

            Assert.That(game.GameVersion, Is.EqualTo(GameVersion.ShadowOfDeath));
            Assert.That(game.Date, Is.EqualTo(new GameDate(month, week, day)));
        }

        [TestCase("conflux_23_dwelling_5", 1, 4, 2)]
        [TestCase("conflux_24_dwelling_6", 1, 4, 3)]
        [TestCase("conflux_25_dwelling_7", 1, 4, 5)]
        [TestCase("conflux_27_upg_dwelling_1", 1, 4, 6)]
        [TestCase("conflux_29_upg_dwelling_3", 2, 1, 1)]
        public void TheDateOfAConfluxSeriesSavegameIsNotTakenFromOtherDataThatLooksLikeOne(string save, int month, int week, int day)
        {
            // Without the marker in front of it the shape of a date also matches something that comes earlier in these savegames
            // (month 2 week 4 day 5, month 2 week 2 day 4, month 60 week 2 day 2).
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Sod", save + ".GM1"));

            Assert.That(game.Date, Is.EqualTo(new GameDate(month, week, day)));
        }

        [Test]
        public void TheDateNeverGoesBackInTheShadowOfDeathSeries()
        {
            var dates = SeriesSaves.Names("Sod", "conflux")
                .Select(n => (Name: n + ".GM1", Date: TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Sod", n + ".GM1")).Date!.Value))
                .ToList();

            Assert.That(dates, Has.Count.EqualTo(35));
            for (var i = 1; i < dates.Count; i++)
            {
                var before = (dates[i - 1].Date.Month * 100) + (dates[i - 1].Date.Week * 10) + dates[i - 1].Date.Day;
                var after = (dates[i].Date.Month * 100) + (dates[i].Date.Week * 10) + dates[i].Date.Day;
                Assert.That(after, Is.GreaterThanOrEqualTo(before), $"{dates[i - 1].Name} {dates[i - 1].Date} then {dates[i].Name} {dates[i].Date}");
            }
        }

        [Test]
        public void TheDateIsFormattedLikeTheAdventureMap()
        {
            var overview = new OverviewViewModel(Load("empire_of_the_world_01_m5w4d6"), _ => { }, _ => { });

            Assert.That(overview.DateText, Is.EqualTo("Month 5, Week 4, Day 6"));
        }

        [Test]
        public void UnknownVersionsAndBytesWithoutADateGiveNone()
        {
            var bytes = TestServices.SavefileCodec.Read(Path.Combine("TestData", "Hota", "empire_of_the_world_01_m5w4d6.GM1"));

            Assert.That(new GameDateLocator().Find(bytes, GameVersion.Unknown), Is.Null);
            Assert.That(new GameDateLocator().Find(new byte[5000], GameVersion.HornOfTheAbyss), Is.Null);
        }
    }
}
