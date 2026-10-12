using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The Shadow of Death hotseat saves in TestData/Sod/Hotseat: two humans (Red and Blue) on a map with Tazar and Sir Mullich of Red
    /// visiting obelisks and bonus buildings. What is stored in the hero when a visit gives a morale or luck bonus is read from the bytes.
    /// </summary>
    [TestFixture]
    internal sealed class SodHotseatTests
    {
        private const int MoraleBonus = 28;
        private const int LuckBonus = 29;
        private const int EffectsA = 135;
        private const int EffectsB = 136;
        private const int EffectsC = 138;

        private static readonly string[] Saves =
        [
            "hotseat_00_baseline_200_diff", "hotseat_01_red_obelisk_1", "hotseat_02_red_obelisk_2", "hotseat_03_tazar_watering_hole", "hotseat_04_tazar_magic_spring", "hotseat_05_tazar_swan_pond",
            "hotseat_06_sir_mullich_stables", "hotseat_07_sir_mullich_fountain_of_fortune", "hotseat_08_sir_mullich_crypt_1", "hotseat_09_sir_mullich_crypt_2", "hotseat_10_sir_mullich_faerie_ring", "hotseat_11_sir_mullich_magic_well",
        ];

        private static HeroChunk Hero(int save, string name)
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Sod", "Hotseat", Saves[save] + ".GM2"));
            var heroes = game.History.Current.Heroes();
            return heroes[heroes.FindByName(name)];
        }

        [Test]
        public void EverySaveLoadsAsShadowOfDeathWithTheSameHeroes()
        {
            foreach (var save in Saves)
            {
                var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Sod", "Hotseat", save + ".GM2"));
                Assert.That(game.GameVersion, Is.EqualTo(GameVersion.ShadowOfDeath), save);
                Assert.That(game.History.Current.Heroes().Count, Is.EqualTo(156), save);
            }
        }

        [Test]
        public void VisitingAnObeliskLeavesTheObeliskObjectTypeUnderTheHero()
        {
            Assert.That(Hero(1, "Tazar").GetByteAt(11), Is.EqualTo(57));
            Assert.That(Hero(1, "Tazar").GetByteAt(15), Is.EqualTo(1));
        }

        [Test]
        public void TheWateringHoleGivesPlusOneMoraleUntilTheNextBattle()
        {
            Assert.That(Hero(2, "Tazar").GetByteAt(MoraleBonus), Is.EqualTo(0));
            Assert.That(Hero(3, "Tazar").GetByteAt(MoraleBonus), Is.EqualTo(1));
            Assert.That(Hero(3, "Tazar").GetByteAt(EffectsA) & 0x40, Is.EqualTo(0x40));
        }

        [Test]
        public void TheSwanPondGivesPlusTwoLuckAndSetsItsEffectBit()
        {
            Assert.That(Hero(4, "Tazar").GetByteAt(LuckBonus), Is.EqualTo(0));
            Assert.That(Hero(5, "Tazar").GetByteAt(LuckBonus), Is.EqualTo(2));
            Assert.That(Hero(5, "Tazar").GetByteAt(EffectsA) & 0x08, Is.EqualTo(0x08));
        }

        [Test]
        public void TheStablesSetTheirEffectBitAndTheFountainOfFortuneGivesLuckWithItsOwn()
        {
            Assert.That(Hero(6, "Sir Mullich").GetByteAt(EffectsA) & 0x02, Is.EqualTo(0x02));
            Assert.That(Hero(7, "Sir Mullich").GetByteAt(LuckBonus), Is.EqualTo(1));
            Assert.That(Hero(7, "Sir Mullich").GetByteAt(EffectsC) & 0x08, Is.EqualTo(0x08));
        }

        [Test]
        public void ABattleClearsTheLuckAndTheFountainBitAndTheSecondCryptVisitGivesMinusOneMorale()
        {
            Assert.That(Hero(8, "Sir Mullich").GetByteAt(LuckBonus), Is.EqualTo(0));
            Assert.That(Hero(8, "Sir Mullich").GetByteAt(EffectsC) & 0x08, Is.EqualTo(0));
            Assert.That(Hero(8, "Sir Mullich").GetByteAt(MoraleBonus), Is.EqualTo(0), "the first visit is the fight");
            Assert.That(Hero(9, "Sir Mullich").GetByteAt(MoraleBonus), Is.EqualTo(0xFF), "morale -1");
            Assert.That(Hero(9, "Sir Mullich").GetByteAt(EffectsB) & 0x04, Is.EqualTo(0x04));
        }

        [Test]
        public void TheTavernOfRedOffersBrissaAndDaceAsTheGameShowed()
        {
            var game = TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Sod", "Hotseat", "five_teams_00_game_begin.GM2"));
            var players = game.History.Current.Players();
            var tavern = players.GetModel(players.FindByColor(PlayerColor.Red)).Tavern;
            var heroes = game.History.Current.Heroes();

            Assert.That(heroes.FindById(tavern.First!.Value)!.Name, Is.EqualTo("Brissa"));
            Assert.That(heroes.FindById(tavern.Second!.Value)!.Name, Is.EqualTo("Dace"));
        }

        private static Game Hotseat(string save) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Sod", "Hotseat", save + ".GM2"));

        [Test]
        public void BothHumansAreFoundInTheSetupAndTheCurrentPlayerFollowsTheTurn()
        {
            var begin = Hotseat("five_teams_00_game_begin");

            Assert.That(begin.PlayerControls, Is.EqualTo(new[]
            {
                PlayerType.Human, PlayerType.Human, PlayerType.Computer, PlayerType.Computer, PlayerType.Computer, PlayerType.Computer, PlayerType.NotInGame, PlayerType.Computer,
            }));
            Assert.That(begin.CurrentPlayer, Is.EqualTo(PlayerColor.Red));
            Assert.That(Hotseat("five_teams_02_blue_two_obelisks").CurrentPlayer, Is.EqualTo(PlayerColor.Blue));
            Assert.That(Hotseat("hotseat_12_blue_turn").CurrentPlayer, Is.EqualTo(PlayerColor.Blue));
        }

        [Test]
        public void BlueIsAPlayerAlthoughItHasNoResourcesAndTealWhichNobodyPlaysIsNot()
        {
            var colors = Hotseat("five_teams_00_game_begin").History.Current.Players().GetColors();

            Assert.That(colors, Is.EqualTo(new[] { PlayerColor.Red, PlayerColor.Blue, PlayerColor.Tan, PlayerColor.Green, PlayerColor.Orange, PlayerColor.Purple, PlayerColor.Pink }));
        }

        [Test]
        public void TheObeliskFlagsAreABitForEveryColourOfTheTeamThatVisited()
        {
            // Five teams: Red alone (Teal, which nobody plays, shares its team by default) and Blue alone. A visit marks the whole team, so Red's is 0x41.
            Assert.That(Hotseat("five_teams_00_game_begin").History.Current.Obelisks().GetModel(), Is.EqualTo(ObeliskVisits.NoneVisited(7)));
            Assert.That(Hotseat("five_teams_01_red_two_obelisks").History.Current.Obelisks().GetModel()!.Masks, Is.EqualTo(new byte[] { 0x41, 0x41, 0, 0, 0, 0, 0 }));
            Assert.That(Hotseat("five_teams_02_blue_two_obelisks").History.Current.Obelisks().GetModel()!.Masks, Is.EqualTo(new byte[] { 0x43, 0x43, 0, 0, 0, 0, 0 }));
        }

        [Test]
        public void EachPlayerSeesOnlyItsOwnVisitsAndEditingOneColourLeavesTheOthersBits()
        {
            var game = Hotseat("five_teams_01_red_two_obelisks");
            var visits = game.History.Current.Obelisks().GetModel()!;

            Assert.That(visits.VisitedCount(PlayerColor.Red), Is.EqualTo(2));
            Assert.That(visits.VisitedCount(PlayerColor.Blue), Is.EqualTo(0));

            game.History.CreateObeliskSession().Update(v => v.WithAll(PlayerColor.Blue, true));
            var after = game.History.Current.Obelisks().GetModel()!;
            Assert.That(after.Masks, Is.EqualTo(new byte[] { 0x43, 0x43, 2, 2, 2, 2, 2 }));
            Assert.That(after.VisitedCount(PlayerColor.Red), Is.EqualTo(2));

            game.History.CreateObeliskSession().Update(v => v.WithAll(PlayerColor.Red, false));
            Assert.That(game.History.Current.Obelisks().GetModel()!.Masks, Is.EqualTo(new byte[] { 0x42, 0x42, 2, 2, 2, 2, 2 }), "the Teal bit stays");
        }

        [Test]
        public void TheTavernOfBlueOffersMalcomAndOrisAsTheGameShowed()
        {
            var game = Hotseat("five_teams_00_game_begin");
            var players = game.History.Current.Players();
            var tavern = players.GetModel(players.FindByColor(PlayerColor.Blue)).Tavern;
            var heroes = game.History.Current.Heroes();

            Assert.That(heroes.FindById(tavern.First!.Value)!.Name, Is.EqualTo("Malcom"));
            Assert.That(heroes.FindById(tavern.Second!.Value)!.Name, Is.EqualTo("Oris"));
        }

        [Test]
        public void EachPlayerPageShowsTheObelisksOfItsOwnColour()
        {
            var game = Hotseat("five_teams_01_red_two_obelisks");
            var players = game.History.Current.Players();
            using var red = new Siniskalc.ViewModels.PlayerViewModel(game.History.CreatePlayerSession(players.FindByColor(PlayerColor.Red)), TestServices.ByteEditTargets);
            using var blue = new Siniskalc.ViewModels.PlayerViewModel(game.History.CreatePlayerSession(players.FindByColor(PlayerColor.Blue)), TestServices.ByteEditTargets);

            Assert.That(red.Obelisks.Summary, Is.EqualTo("2 of 7 visited"));
            Assert.That(blue.Obelisks.Summary, Is.EqualTo("0 of 7 visited"));
            Assert.That(blue.Obelisks.Obelisks.Select(o => o.IsVisited), Has.All.False);

            blue.Obelisks.Obelisks[3].IsVisited = true;
            Assert.That(blue.Obelisks.Summary, Is.EqualTo("1 of 7 visited"));
            Assert.That(red.Obelisks.Summary, Is.EqualTo("2 of 7 visited"), "the other player's page is not changed");
        }

        [Test]
        public void TheObelisksOfTheHotseatMapHaveCoordinates()
        {
            var visits = Hotseat("five_teams_00_game_begin").History.Current.Obelisks().GetModel()!;

            Assert.That(
                Enumerable.Range(0, visits.Count).Select(i => $"{visits.PositionOf(i)!.X},{visits.PositionOf(i)!.Y},{visits.PositionOf(i)!.Z}"),
                Is.EqualTo(new[] { "47,8,0", "46,11,0", "5,28,0", "34,58,0", "9,61,0", "54,66,0", "34,69,0" }));
        }

        [Test]
        public void TheTilesOfTheObelisksNameTheirPlaceWithTheLevel()
        {
            var game = Hotseat("five_teams_00_game_begin");
            using var red = new Siniskalc.ViewModels.PlayerViewModel(game.History.CreatePlayerSession(game.History.Current.Players().FindByColor(PlayerColor.Red)), TestServices.ByteEditTargets);

            Assert.That(red.Obelisks.Obelisks[0].Coordinates, Is.EqualTo("X: 47, Y: 8, Surface (0)"));
        }
    }
}
