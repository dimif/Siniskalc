using Siniskalc;
using Siniskalc.Entities;
using Siniskalc.Data;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The Arts saves were made in turn: "arts_00_before_arts", then arts_01_arts (Valeska, Edric, Christian and Ingham moved; Valeska had already moved before), arts_03_arts (a new day),
    /// and arts_04_arts (Ingham moved again). The flag that changed with it is byte 1095 of the hero.
    /// </summary>
    [TestFixture]
    internal sealed class MovedTodayTests
    {
        private static Game Load(string name) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Arts", name + ".GM1"));

        private static HeroEditSession Session(Game game, string hero) => game.History.CreateHeroSession(game.History.Current.Heroes().FindByName(hero));

        [TestCase("arts_00_before_arts", "Ingham", false)]
        [TestCase("arts_00_before_arts", "Christian", false)]
        [TestCase("arts_00_before_arts", "Valeska", true)]
        [TestCase("arts_01_arts", "Christian", true)]
        [TestCase("arts_01_arts", "Ingham", true)]
        [TestCase("arts_02_arts", "Christian", true)]
        [TestCase("arts_03_arts", "Christian", false)]
        [TestCase("arts_03_arts", "Ingham", false)]
        [TestCase("arts_04_arts", "Ingham", true)]
        [TestCase("arts_04_arts", "Christian", false)]
        public void TheFlagFollowsTheHeroMovingAndTheNewDay(string save, string hero, bool expected)
        {
            Assert.That(Session(Load(save), hero).Current.MovedToday, Is.EqualTo(expected));
        }

        [Test]
        public void SettingTheFlagChangesOneByteAndItSurvivesSavingAndLoading()
        {
            var game = Load("arts_00_before_arts");
            Session(game, "Christian").Update(h => h with { MovedToday = true });

            var saved = game.GetBytesToSave();
            var changed = Enumerable.Range(0, saved.Length).Where(i => saved[i] != game.Bytes[i]).ToList();
            var chunk = game.History.Current.Heroes().Single(c => c.Name == "Christian");

            Assert.That(changed, Is.EqualTo(new[] { chunk.StartIndex + 1095 }));
            Assert.That(Session(TestServices.GameService.LoadGameFromBytes(saved), "Christian").Current.MovedToday, Is.True);
        }

        [Test]
        public void TheFlagCanBeClearedAndTheChangeIsListedAndReverted()
        {
            var game = Load("arts_01_arts");
            Session(game, "Christian").Update(h => h with { MovedToday = false });

            var change = game.History.GetPendingChanges().Single();

            Assert.That(change.Change.Description, Is.EqualTo("Moved today"));
            Assert.That(change.Change.Before, Is.EqualTo("yes"));
            Assert.That(change.Change.After, Is.EqualTo("no"));
            change.Revert(game.History);
            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
        }

        [Test]
        public void OnlyHotaIsVerifiedButEveryVersionHasTheFlag()
        {
            var classic = new HeroOffsets(GameVersion.ShadowOfDeath);

            Assert.That(classic.AreDayFlagsVerified, Is.False);
            Assert.That(new HeroOffsets(GameVersion.HornOfTheAbyss).AreDayFlagsVerified, Is.True);
            Assert.That(TestServices.HeroRegions.GetRegions(classic).Select(r => r.Name), Does.Contain("Moved today"));
            Assert.That(TestServices.HeroRegions.GetRegions(new HeroOffsets(GameVersion.HornOfTheAbyss)).Select(r => r.Name), Does.Contain("Moved today"));
        }
    }
}
