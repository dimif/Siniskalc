using Siniskalc.Homm3.Tests.Data;
namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// The mines of the test map in <c>TestData/Hota/Mines</c>: Red takes one a day and Blue takes it from Red the next turn.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MineTests
    {
        // The mine's own trailing coordinates, rather than the preceding record's coordinates, identify the captured row.
        private const int FirstLab = 5;
        private const int FirstSulfur = 17;
        private const int AbandonedSulfur = 3;
        private const int AbandonedGold = 4;

        private static string Path(string name) => System.IO.Path.Combine("TestData", "Hota", "Mines", name + ".GM2");

        private static Siniskalc.Homm3.Runtime.Game Load(string name) => SavedGames.Get(Path(name));

        private static IReadOnlyList<MineModel> Mines(string name) => Load(name).History.Current.Mines().GetModels();

        private static string Owners(string name) => string.Join(string.Empty, Mines(name).Select(m => m.Owner == PlayerColor.Neutral ? '-' : m.Owner.Name[0]));

        [Test]
        public void TheMinesOfTheTestMapAreFoundInEverySaveAndNobodyOwnsThemAtTheStart()
        {
            foreach (var file in Directory.GetFiles(System.IO.Path.Combine("TestData", "Hota", "Mines"), "*.GM2"))
            {
                Assert.That(Mines(System.IO.Path.GetFileNameWithoutExtension(file)), Has.Count.EqualTo(19), file);
            }

            var start = Mines("mines_00_game_begin");
            Assert.That(start.Select(m => m.Kind).Distinct().Count(), Is.EqualTo(7));
            Assert.That(start.Count(m => m.Owner != PlayerColor.Neutral), Is.EqualTo(0));
            Assert.That(start[FirstSulfur].Kind, Is.EqualTo(MineKind.SulfurMine));
            Assert.That(start[FirstSulfur].Position, Is.EqualTo(new MapPosition(30, 14)));
            Assert.That(start[0].Position, Is.EqualTo(new MapPosition(14, 62)), "the first record used to be lost");
        }

        [Test]
        public void RedTakesTheMinesOfTheFirstRowOneAtATimeAndBlueTakesEachFromHimTheNextTurn()
        {
            Assert.That(Owners("mines_00_game_begin"), Is.EqualTo("-------------------"));
            Assert.That(Owners("mines_01_red_sulfur_mine_1"), Is.EqualTo("-----------------R-"));
            Assert.That(Owners("mines_02_red_sawmill_1"), Is.EqualTo("---------------R-B-"));
            Assert.That(Owners("mines_07_red_alchemists_lab_1"), Is.EqualTo("-----R-B-B-B-B-B-B-"));
            Assert.That(Owners("mines_08_red_abandoned_mine_sulfur"), Is.EqualTo("---R-B-B-B-B-B-B-B-"));
        }

        [Test]
        public void TheSecondRowIsTakenInTheReverseOrderAndAnAbandonedMineKeepsItsRecordWhenItIsTaken()
        {
            Assert.That(Owners("mines_09_red_abandoned_mine_gold"), Is.EqualTo("---BRB-B-B-B-B-B-B-"));
            Assert.That(Owners("mines_17_red_sulfur_mine_2"), Is.EqualTo("---BBBBBBBBBBBBBBBR"));
        }

        [Test]
        public void AnAbandonedMineIsGuardedUntilItIsTaken()
        {
            var before = Mines("mines_00_game_begin")[AbandonedSulfur];
            var after = Mines("mines_08_red_abandoned_mine_sulfur")[AbandonedSulfur];

            Assert.That(before.Kind, Is.EqualTo(MineKind.SulfurMine));
            Assert.That(before.Guard.Filled.Count(), Is.GreaterThan(0));
            Assert.That(after.Owner, Is.EqualTo(PlayerColor.Red));
            Assert.That(after.Guard.Filled.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ChangingTheOwnerWritesOnlyTheOwnerByteAndCanBePutBack()
        {
            var game = Load("mines_01_red_sulfur_mine_1");
            var chunk = game.History.Current.Mines()[FirstSulfur];

            game.History.CreateMineSession(FirstSulfur).Update(m => m with { Owner = PlayerColor.Green });

            var saved = TestServices.SavefileCodec.Read(Path("mines_01_red_sulfur_mine_1"));
            var written = game.GetBytesToSave();
            var changed = Enumerable.Range(0, saved.Length).Where(i => saved[i] != written[i]).ToList();
            Assert.That(changed, Is.EqualTo(new[] { chunk.StartIndex + MineLayout.Owner }));
            Assert.That(written[chunk.StartIndex + MineLayout.Owner], Is.EqualTo((byte)PlayerColor.Green.Code));
            Assert.That(game.History.GetPendingChanges().Single().Change.After, Is.EqualTo("Green"));

            game.History.GetPendingChanges().Single().Revert(game.History);
            Assert.That(game.History.Current.Mines().GetModel(FirstSulfur).Owner, Is.EqualTo(PlayerColor.Red));
        }

        [Test]
        public void TheGuardOfAMineIsEditedLikeAnArmyAndWrittenBack()
        {
            var game = Load("mines_00_game_begin");
            var pikeman = TestCatalog.For(GameVersion.HornOfTheAbyss).Creatures.ById("Pikeman");

            game.History.CreateMineSession(FirstLab).Update(m => m with { Guard = m.Guard.WithSlot(0, pikeman, 12) });

            var again = TestServices.GameService.LoadGameFromBytes(game.GetBytesToSave()).History.Current.Mines().GetModel(FirstLab);
            Assert.That(again.Guard.Slots[0]!.Creature, Is.EqualTo(pikeman));
            Assert.That(again.Guard.Slots[0]!.Count, Is.EqualTo(12));
            Assert.That(game.History.GetPendingChanges().Single().Change.Area, Is.EqualTo(ChangeAreas.Garrison));
        }

        [Test]
        public void EveryMineOfEverySaveIsWrittenBackAsTheSameBytes()
        {
            foreach (var file in Directory.GetFiles(System.IO.Path.Combine("TestData", "Hota", "Mines"), "*.GM2"))
            {
                var mines = SavedGames.Get(file).History.Current.Mines();
                foreach (var chunk in mines)
                {
                    Assert.That(mines.Serializer.Serialize(chunk, mines.Deserializer.Deserialize(chunk)).HasSameBytes(chunk), Is.True, $"{file}: {chunk.Name}");
                }
            }
        }
    }
}
