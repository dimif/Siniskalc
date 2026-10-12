using Siniskalc.Data;
using Siniskalc.Entities;
using Siniskalc.Models;
using Siniskalc.Runtime;

namespace Siniskalc.Tests.Data
{
    /// <summary>
    /// The Horn of the Abyss saves in TestData/Hota/SleepGarrisonVisiting: Red's castle Ellesmere, with Akka (id 211) and Oidana (id 199).
    /// 1 Akka visits, 2 she is garrisoned, 3 she visits again and is asleep, 4 she is garrisoned and Oidana visits, 5 Oidana has moved away,
    /// 6 and 7 the two heroes swap places (Oidana walked back to the castle first), 8 Oidana has moved away and is asleep.
    /// </summary>
    [TestFixture]
    internal sealed class HotaSleepGarrisonVisitingTests
    {
        private const int Akka = 211;
        private const int Oidana = 199;
        private const int OnMap = 6;

        private static readonly string[] Saves =
        [
            "sleep_garrison_visiting_01_akka_visiting", "sleep_garrison_visiting_02_akka_garrisoned", "sleep_garrison_visiting_03_akka_asleep", "sleep_garrison_visiting_04_akka_garrisoned_oidana_visiting", "sleep_garrison_visiting_05_oidana_not_visiting",
            "sleep_garrison_visiting_06_garrison_visit_swap1", "sleep_garrison_visiting_07_garrison_visit_swap2", "sleep_garrison_visiting_08_oidana_asleep",
        ];

        private static Game Load(string save) => TestServices.GameService.LoadGameFromFile(Path.Combine("TestData", "Hota", "SleepGarrisonVisiting", save + ".GM1"));

        private static HeroChunk Chunk(Game game, string name) => game.History.Current.Heroes()[game.History.Current.Heroes().FindByName(name)];

        private static CastleModel Ellesmere(Game game)
        {
            var castles = game.History.Current.Castles();
            return TestServices.CastleDeserializer.Deserialize(castles[castles.FindByName("Ellesmere")]);
        }

        [TestCase("sleep_garrison_visiting_01_akka_visiting", null, Akka)]
        [TestCase("sleep_garrison_visiting_02_akka_garrisoned", Akka, null)]
        [TestCase("sleep_garrison_visiting_03_akka_asleep", null, Akka)]
        [TestCase("sleep_garrison_visiting_04_akka_garrisoned_oidana_visiting", Akka, Oidana)]
        [TestCase("sleep_garrison_visiting_05_oidana_not_visiting", Akka, null)]
        [TestCase("sleep_garrison_visiting_06_garrison_visit_swap1", Oidana, Akka)]
        [TestCase("sleep_garrison_visiting_07_garrison_visit_swap2", Akka, Oidana)]
        [TestCase("sleep_garrison_visiting_08_oidana_asleep", Akka, null)]
        public void TheCastleHoldsTheGarrisonedAndTheVisitingHero(string save, int? garrisoned, int? visiting)
        {
            var castle = Ellesmere(Load(save));

            Assert.That((castle.GarrisonedHeroId, castle.VisitingHeroId), Is.EqualTo((garrisoned, visiting)));
        }

        [Test]
        public void AHeroInsideTheCastleIsOffTheMapAndOneAtTheGateIsOnIt()
        {
            // Byte 6 of the hero (the same as in Shadow of Death): 1 for a hero on the map, a visiting one too, 0 for one inside the castle.
            foreach (var (save, akka, oidana) in new[]
            {
                ("sleep_garrison_visiting_01_akka_visiting", 1, (int?)null), ("sleep_garrison_visiting_02_akka_garrisoned", 0, null), ("sleep_garrison_visiting_03_akka_asleep", 1, null), ("sleep_garrison_visiting_04_akka_garrisoned_oidana_visiting", 0, 1),
                ("sleep_garrison_visiting_05_oidana_not_visiting", 0, 1), ("sleep_garrison_visiting_06_garrison_visit_swap1", 1, 0), ("sleep_garrison_visiting_07_garrison_visit_swap2", 0, 1), ("sleep_garrison_visiting_08_oidana_asleep", 0, 1),
            })
            {
                var game = Load(save);
                Assert.That(Chunk(game, "Akka").GetByteAt(OnMap), Is.EqualTo(akka), $"{save}: Akka");
                if (oidana != null)
                {
                    Assert.That(Chunk(game, "Oidana").GetByteAt(OnMap), Is.EqualTo(oidana), $"{save}: Oidana");
                }
            }
        }

        [Test]
        public void TheAsleepFlagStaysSetWhileTheHeroIsGarrisonedAndIsInTheChunk()
        {
            foreach (var save in Saves)
            {
                var game = Load(save);
                var number = SeriesSaves.Number(save);
                var akka = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Akka")).Current;

                Assert.That(akka.IsAsleep, Is.EqualTo(number >= 3), $"{save}: Akka");
                Assert.That(Chunk(game, "Akka").GetByteAt(HeroOffsets.HotaAsleepOffset), Is.EqualTo(number >= 3 ? 1 : 0), save);
                if (number >= 4)
                {
                    var oidana = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Oidana")).Current;
                    Assert.That(oidana.IsAsleep, Is.EqualTo(number == 8), $"{save}: Oidana");
                }
            }
        }

        [Test]
        public void OnlyTheHeroThatMovedHasTheMovedTodayFlag()
        {
            // Horn of the Abyss has a flag for it (byte 1095), which Oidana got in save 5 when she left the castle and kept.
            foreach (var save in Saves.Skip(4))
            {
                var game = Load(save);
                var oidana = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Oidana")).Current;
                var akka = game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Akka")).Current;

                Assert.That(oidana.MovedToday, Is.True, save);
                Assert.That(oidana.MovementPoints.CurrentMovementPoints, Is.LessThan(oidana.MovementPoints.MaxMovementPoints), save);
                Assert.That(akka.MovedToday, Is.False, save);
            }

            var before = Load("sleep_garrison_visiting_04_akka_garrisoned_oidana_visiting");
            Assert.That(before.History.CreateHeroSession(before.History.Current.Heroes().FindByName("Oidana")).Current.MovedToday, Is.False);
        }

        [TestCase("sleep_garrison_visiting_04_akka_garrisoned_oidana_visiting", 1, Oidana, Oidana)]
        [TestCase("sleep_garrison_visiting_05_oidana_not_visiting", 1, Oidana, Oidana)]
        [TestCase("sleep_garrison_visiting_06_garrison_visit_swap1", 1, Akka, Akka)]
        [TestCase("sleep_garrison_visiting_07_garrison_visit_swap2", 1, Oidana, Oidana)]
        [TestCase("sleep_garrison_visiting_08_oidana_asleep", 1, Oidana, Oidana)]
        public void TheListOfHeroesOfThePlayerHoldsTheHeroAtTheGateAndNotTheGarrisonedOne(string save, int count, int firstId, int selected)
        {
            var game = Load(save);
            var red = game.History.Current.Players().Single(p => p.Name == "Red");

            Assert.That(red.GetByteAt(PlayerLayout.HeroCount), Is.EqualTo(count));
            Assert.That(red.GetByteAt(PlayerLayout.HeroIds), Is.EqualTo(firstId));
            Assert.That(red.GetByteAt(PlayerLayout.SelectedHero), Is.EqualTo(selected));
            Assert.That(red.GetByteAt(PlayerLayout.HeroIds + 1), Is.EqualTo(0xFF));
        }

        [Test]
        public void ASwapChangesTheCastleTheFlagsOfBothHeroesAndTheListOfHeroesTogether()
        {
            var before = Load("sleep_garrison_visiting_05_oidana_not_visiting");
            var first = Load("sleep_garrison_visiting_06_garrison_visit_swap1");
            var second = Load("sleep_garrison_visiting_07_garrison_visit_swap2");

            // Save 5 to 6: Oidana walked back to the castle and Akka came out. Akka's own bytes change in the flag only.
            Assert.That(Changed(Chunk(before, "Akka"), Chunk(first, "Akka")), Is.EqualTo(new[] { OnMap }));
            Assert.That(Changed(Chunk(first, "Akka"), Chunk(second, "Akka")), Is.EqualTo(new[] { OnMap }), "and back in again");

            // 6 to 7 is the swap itself, with nothing else done: the flag of Oidana and her facing, as she comes out.
            Assert.That(Changed(Chunk(first, "Oidana"), Chunk(second, "Oidana")), Is.EqualTo(new[] { OnMap, 41 }));
            Assert.That(Ellesmere(first).GarrisonedHeroId, Is.EqualTo(Oidana));
            Assert.That(Ellesmere(second).GarrisonedHeroId, Is.EqualTo(Akka));
            Assert.That(first.History.Current.Players().Single(p => p.Name == "Red").GetByteAt(PlayerLayout.HeroIds), Is.EqualTo(Akka));
            Assert.That(second.History.Current.Players().Single(p => p.Name == "Red").GetByteAt(PlayerLayout.HeroIds), Is.EqualTo(Oidana));
        }

        private static List<int> Changed(HeroChunk a, HeroChunk b) => Enumerable.Range(0, a.Length).Where(i => a.GetByteAt(i) != b.GetByteAt(i)).ToList();

        [Test]
        public void RedIsFoundAsAPlayerAlthoughItHasNoResourcesBecauseItHasAHeroAndATown()
        {
            // Saves 1 to 3 have Akka of Red and a town, and Red has no resources at all until save 4.
            foreach (var save in Saves)
            {
                var game = Load(save);
                Assert.That(game.History.Current.Players().GetColors(), Does.Contain(PlayerColor.Red), save);
                Assert.That(game.History.CreateHeroSession(game.History.Current.Heroes().FindByName("Akka")).Current.Owner, Is.EqualTo(PlayerColor.Red), save);
            }
        }
    }
}
