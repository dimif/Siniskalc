using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// In the Arts saves 7 to 12, Ingham cast Water Walk in arts_06_arts, which is still active in 7 and 8 (same day). He has the tomes for the adventure spells and casts them: 8 Disguise, 9 Visions (a new day came
    /// first), 10 Water Walk, 11 Fly, 12 all four on the same day.
    /// </summary>
    [TestFixture]
    internal sealed class ActiveSpellsTests
    {
        private static Game Load(string name) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "Arts", name + ".GM1"));

        private static HeroEditSession Session(Game game, string hero) => game.History.CreateHeroSession(game.History.Current.Heroes().FindByName(hero));

        [TestCase("arts_06_arts", ActiveAdventureSpells.WaterWalk)]
        [TestCase("arts_07_before_adv_spells", ActiveAdventureSpells.WaterWalk)]
        [TestCase("arts_08_disguise", ActiveAdventureSpells.Disguise | ActiveAdventureSpells.WaterWalk)]
        [TestCase("arts_09_vision", ActiveAdventureSpells.Visions)]
        [TestCase("arts_10_waterwalk", ActiveAdventureSpells.WaterWalk)]
        [TestCase("arts_11_fly", ActiveAdventureSpells.Fly)]
        [TestCase("arts_12_all", ActiveAdventureSpells.Disguise | ActiveAdventureSpells.Fly | ActiveAdventureSpells.WaterWalk | ActiveAdventureSpells.Visions)]
        public void TheSpellsInghamCastTodayAreRead(string save, ActiveAdventureSpells expected)
        {
            Assert.That(Session(Load(save), "Ingham").Current.ActiveSpells, Is.EqualTo(expected));
        }

        [Test]
        public void NoOtherHeroHasCastAnything()
        {
            var game = Load("arts_12_all");

            foreach (var chunk in game.History.Current.Heroes().Where(c => c.Name != "Ingham"))
            {
                Assert.That(Session(game, chunk.Name).Current.ActiveSpells, Is.EqualTo(ActiveAdventureSpells.None), chunk.Name);
            }
        }

        [Test]
        public void CastingASpellSetsTheByteTheGameSetsAndNothingElseChanges()
        {
            var game = Load("arts_07_before_adv_spells");
            Session(game, "Ingham").Update(h => h with { ActiveSpells = ActiveAdventureSpells.Fly | ActiveAdventureSpells.WaterWalk });

            var saved = game.GetBytesToSave();
            var changed = Enumerable.Range(0, saved.Length).Where(i => saved[i] != game.Bytes[i]).ToList();
            var chunk = game.History.Current.Heroes().Single(c => c.Name == "Ingham");

            Assert.That(changed, Is.EqualTo(new[] { chunk.StartIndex + 32 }));
            Assert.That(Session(TestServices.GameService.LoadGameFromBytes(saved), "Ingham").Current.ActiveSpells, Is.EqualTo(ActiveAdventureSpells.Fly | ActiveAdventureSpells.WaterWalk));
        }

        [Test]
        public void ClearingTheSpellsRestoresTheBytesOfTheSaveBeforeTheCasting()
        {
            var game = Load("arts_12_all");
            Session(game, "Ingham").Update(h => h with { ActiveSpells = ActiveAdventureSpells.None });
            var saved = game.GetBytesToSave();
            var chunk = game.History.Current.Heroes().Single(c => c.Name == "Ingham");

            Assert.That(saved.Skip(chunk.StartIndex + 31).Take(5), Is.EqualTo(new byte[] { 255, 255, 255, 0, 255 }));
        }

        [Test]
        public void EachChangeIsListedAndCanBeReverted()
        {
            var game = Load("arts_12_all");
            Session(game, "Ingham").Update(h => h with { ActiveSpells = ActiveAdventureSpells.Fly });

            var changes = game.History.GetPendingChanges();

            Assert.That(changes.Select(c => c.Change.Description), Is.EquivalentTo(new[] { "Disguise cast today", "WaterWalk cast today", "Visions cast today" }));
            foreach (var change in changes.ToList())
            {
                change.Revert(game.History);
            }

            Assert.That(game.GetBytesToSave(), Is.EqualTo(game.Bytes));
        }

        [Test]
        public void EveryVersionHasTheBytesButOnlyHotaIsVerified()
        {
            var classic = new HeroOffsets(GameVersion.ShadowOfDeath);

            Assert.That(classic.ActiveSpellBytes, Has.Count.EqualTo(4));
            Assert.That(classic.AreDayFlagsVerified, Is.False);
            Assert.That(TestServices.HeroRegions.GetRegions(classic).Select(r => r.Name), Does.Contain("Active adventure spells"));
            Assert.That(TestServices.HeroRegions.GetRegions(new HeroOffsets(GameVersion.HornOfTheAbyss)).Select(r => r.Name), Does.Contain("Active adventure spells"));
        }
    }
}
