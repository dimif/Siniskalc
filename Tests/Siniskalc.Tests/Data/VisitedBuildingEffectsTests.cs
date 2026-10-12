using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The morale and luck that visited buildings gave a hero, and the bits that remember the buildings, read from TestData/Sod/Hotseat.
    /// </summary>
    [TestFixture]
    internal sealed class VisitedBuildingEffectsTests
    {
        private static readonly string[] Saves =
        [
            "hotseat_00_baseline_200_diff", "hotseat_01_red_obelisk_1", "hotseat_02_red_obelisk_2", "hotseat_03_tazar_watering_hole", "hotseat_04_tazar_magic_spring", "hotseat_05_tazar_swan_pond",
            "hotseat_06_sir_mullich_stables", "hotseat_07_sir_mullich_fountain_of_fortune", "hotseat_08_sir_mullich_crypt_1", "hotseat_09_sir_mullich_crypt_2", "hotseat_10_sir_mullich_faerie_ring", "hotseat_11_sir_mullich_magic_well",
        ];

        private static Game Load(int save) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Sod", "Hotseat", Saves[save] + ".GM2"));

        private static HeroEditSession Session(Game game, string name) => game.History.CreateHeroSession(game.History.Current.Heroes().FindByName(name));

        private static HeroModel Hero(int save, string name) => Session(Load(save), name).Current;

        [Test]
        public void TheWateringHoleGivesPlusOneMoraleAndTheSwanPondPlusTwoLuck()
        {
            Assert.That(Hero(3, "Tazar").MoraleBonus, Is.EqualTo(1));
            Assert.That(Hero(3, "Tazar").VisitEffects, Is.EqualTo(VisitedBuildingEffects.WateringHole));
            Assert.That(Hero(5, "Tazar").LuckBonus, Is.EqualTo(2));
            Assert.That(Hero(5, "Tazar").VisitEffects, Is.EqualTo(VisitedBuildingEffects.WateringHole | VisitedBuildingEffects.SwanPond));
        }

        [Test]
        public void TheCryptTheFountainAndTheStablesAreRead()
        {
            Assert.That(Hero(6, "Sir Mullich").VisitEffects, Is.EqualTo(VisitedBuildingEffects.Stables));
            Assert.That(Hero(7, "Sir Mullich").VisitEffects, Is.EqualTo(VisitedBuildingEffects.Stables | VisitedBuildingEffects.FountainOfFortune));
            Assert.That(Hero(7, "Sir Mullich").LuckBonus, Is.EqualTo(1));
            Assert.That(Hero(9, "Sir Mullich").MoraleBonus, Is.EqualTo(-1));
            Assert.That(Hero(9, "Sir Mullich").VisitEffects.HasFlag(VisitedBuildingEffects.Crypt), Is.True);
        }

        [Test]
        public void TheFaerieRingIsReadAndABattleClearsTheFountain()
        {
            Assert.That(Hero(10, "Sir Mullich").VisitEffects.HasFlag(VisitedBuildingEffects.FaerieRing), Is.True);
            Assert.That(Hero(8, "Sir Mullich").VisitEffects.HasFlag(VisitedBuildingEffects.FountainOfFortune), Is.False);
        }

        [Test]
        public void TheIdolOfFortuneIsReadInTheRestorationOfErathiaSeries()
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Roe", "roe_63_tazar_idle_of_fortune.GM1"));

            // Tazar also visited the stables earlier in the week; their bit is gone in save 76, the first of the next week.
            Assert.That(Session(game, "Tazar").Current.VisitEffects, Is.EqualTo(VisitedBuildingEffects.Stables | VisitedBuildingEffects.IdolOfFortune));
        }

        [Test]
        public void TheEffectsMoraleAndLuckAreReadOnlyAndNeverWritten()
        {
            var game = Load(9);
            var session = Session(game, "Sir Mullich");
            var heroes = game.History.Current.Heroes();

            session.Update(h => h with { VisitEffects = VisitedBuildingEffects.None, MoraleBonus = 3, LuckBonus = 3 });

            Assert.That(game.History.GetPendingChanges(), Is.Empty);
            Assert.That(heroes[heroes.FindByName("Sir Mullich")].GetByteAt(28), Is.EqualTo(0xFF));
            Assert.That(Session(game, "Sir Mullich").Current.VisitEffects.HasFlag(VisitedBuildingEffects.Crypt), Is.True);
        }

        [Test]
        public void TheMoraleAndLuckOfAHeroNameOnlyTheModifiersThatAreNotZero()
        {
            var game = Load(5);
            using var tazar = TestServices.CreateHeroViewModel(Session(game, "Tazar"));
            var general = tazar.GeneralInfoViewModel;

            Assert.That(general.LuckBreakdown, Does.Contain("buildings +2"));
            Assert.That(general.LuckBreakdown, Does.Not.Contain("artifacts"));
            Assert.That(general.MoraleBreakdown, Does.Contain("buildings +1"));
            Assert.That(general.MoraleBreakdown, Does.Not.Contain(" 0"));
            Assert.That(general.WateringHoleVisited && general.SwanPondVisited && !general.CryptVisited, Is.True);

            using var untouched = TestServices.CreateHeroViewModel(Session(Load(0), "Tazar"));
            Assert.That(untouched.GeneralInfoViewModel.LuckBreakdown, Is.Empty);
            Assert.That(untouched.GeneralInfoViewModel.MoraleText, Does.Match("^[+-][0-9]$"));
        }

        [TestCase(0)]
        [TestCase(9)]
        [TestCase(11)]
        public void WritingBackAHeroThatWasNotChangedKeepsEveryByteOfItsChunk(int save)
        {
            var game = Load(save);
            var heroes = game.History.Current.Heroes();
            for (var i = 0; i < heroes.Count; i++)
            {
                var before = heroes[i];
                game.History.CreateHeroSession(i).Update(h => h with { VisitEffects = h.VisitEffects });

                Assert.That(game.History.Current.Heroes()[i].HasSameBytes(before), Is.True, $"{before.Name} in {Saves[save]}");
            }
        }
    }
}
