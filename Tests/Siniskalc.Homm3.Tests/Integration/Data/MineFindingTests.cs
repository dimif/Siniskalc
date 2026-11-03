using Siniskalc.Core.Data;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    /// <summary>The small-map capture controls supplied to establish counted mine-table boundaries.</summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class MineFindingTests
    {
        private const string Root = "TestData\\Hota\\MineFinding";

        private static IEnumerable<string> Saves => Directory.GetFiles(Root, "*.GM1", SearchOption.AllDirectories);

        private static string Save(string folder, string name) => Path.Combine(Root, folder, name + ".GM1");

        [TestCase("NoMinesOneDwelling", 0)]
        [TestCase("OneMineNoDwellings", 1)]
        [TestCase("OneMineOneDwelling", 1)]
        [TestCase("OneMineTwoDwellings", 1)]
        [TestCase("TwoMinesTwoDwellings", 2)]
        [TestCase("TwoMinesThreeDwellings", 2)]
        public void EveryCaptureSaveOfASmallMapHasExactlyItsMinesAndNoFalseMatches(string folder, int expected)
        {
            foreach (var file in Directory.GetFiles(Path.Combine(Root, folder), "*.GM1"))
            {
                var game = SavedGames.Get(file);
                Assert.That(game.History.Current.Mines(), Has.Count.EqualTo(expected), file);
            }
        }

        [TestCase("OneMineNoDwellings", "one_mine_no_dwellings_00_baseline", "one_mine_no_dwellings_01_red_captures_mine", 0)]
        [TestCase("OneMineOneDwelling", "one_mine_one_dwelling_00_baseline", "one_mine_one_dwelling_01_red_captures_mine", 0)]
        [TestCase("OneMineTwoDwellings", "one_mine_two_dwellings_00_baseline", "one_mine_two_dwellings_01_red_captures_mine", 0)]
        [TestCase("TwoMinesTwoDwellings", "two_mines_two_dwellings_00_baseline", "two_mines_two_dwellings_01_red_captures_sulfur_mine", 1)]
        [TestCase("TwoMinesTwoDwellings", "two_mines_two_dwellings_01_red_captures_sulfur_mine", "two_mines_two_dwellings_02_red_captures_gold_mine", 0)]
        [TestCase("TwoMinesThreeDwellings", "two_mines_three_dwellings_00_baseline", "two_mines_three_dwellings_01_red_captures_sulfur_mine", 0)]
        [TestCase("TwoMinesThreeDwellings", "two_mines_three_dwellings_01_red_captures_sulfur_mine", "two_mines_three_dwellings_02_red_captures_gold_mine", 1)]
        public void EachCaptureChangesOnlyItsOwnOwnerWithinTheCountedMineRecords(string folder, string before, string after, int captured)
        {
            var oldMines = SavedGames.Get(Save(folder, before)).History.Current.Mines();
            var newMines = SavedGames.Get(Save(folder, after)).History.Current.Mines();
            Assert.That(newMines, Has.Count.EqualTo(oldMines.Count));
            for (var index = 0; index < oldMines.Count; index++)
            {
                var expected = index == captured ? new[] { (MineLayout.Owner, (byte)255, (byte)0) } : Array.Empty<(int, byte, byte)>();
                Assert.That(ByteDiff.Of(oldMines[index].ToArray(), newMines[index].ToArray()), Is.EqualTo(expected));
            }
        }

        [Test]
        public void TheTwoMinesUseTheirOwnTrailingCoordinatesRatherThanTheCountOrTheirNeighbours()
        {
            var mines = SavedGames.Get(Save("TwoMinesThreeDwellings", "two_mines_three_dwellings_00_baseline")).History.Current.Mines();
            Assert.That(mines.GetModels().Select(m => (m.Kind, m.Position)), Is.EqualTo(new[]
            {
                (MineKind.SulfurMine, new MapPosition(3, 8)),
                (MineKind.GoldMine, new MapPosition(7, 8)),
            }));
            Assert.That(mines[1].StartIndex - mines[0].StartIndex, Is.EqualTo(62));
        }

        [TestCaseSource(nameof(Saves))]
        public void EverySmallMapSaveAndItsMineRecordsRoundTripWithoutChangingAnyBytes(string file)
        {
            var game = SavedGames.Get(file);
            var mines = game.History.Current.Mines();
            foreach (var chunk in mines)
            {
                Assert.That(mines.Serializer.Serialize(chunk, mines.Deserializer.Deserialize(chunk)).ToArray(), Is.EqualTo(chunk.ToArray()));
            }

            Assert.That(game.GetBytesToSave(), Is.EqualTo(TestServices.SavefileCodec.Read(file)));
        }

        [Test]
        public void EditingEachMinePreservesTheCountItsCoordinatesTheOtherMineAndAllDwellingBytes()
        {
            var file = Save("TwoMinesThreeDwellings", "two_mines_three_dwellings_00_baseline");
            foreach (var index in new[] { 0, 1 })
            {
                var game = SavedGames.Get(file);
                var history = game.History;
                var before = history.Current.ToBytes();
                var start = history.Current.Mines()[index].StartIndex;
                var session = history.CreateMineSession(index);
                session.Update(m => m with { Owner = PlayerColor.Blue });
                Assert.That(ByteDiff.Of(before, game.GetBytesToSave()), Is.EqualTo(new[] { (start, (byte)255, (byte)1) }));
                history.Undo();
                Assert.That(game.GetBytesToSave(), Is.EqualTo(before));
                history.Redo();
                Assert.That(history.Current.Mines().GetModel(index).Owner, Is.EqualTo(PlayerColor.Blue));
                history.GetPendingChanges().Single().Revert(history);
                Assert.That(game.GetBytesToSave(), Is.EqualTo(before));
                var creature = history.Current.Catalog().Creatures.ById("Pikeman");
                session.Update(m => m with { Guard = m.Guard.WithSlot(0, creature, 12) });
                var allowed = Enumerable.Range(start + MineLayout.Guard.TypeAt(0), 4).Concat(Enumerable.Range(start + MineLayout.Guard.CountAt(0), 4));
                Assert.That(ByteDiff.Of(before, game.GetBytesToSave()).Select(d => d.Offset), Is.Not.Empty.And.SubsetOf(allowed));
                var reloaded = TestServices.GameService.LoadGameFromBytes(game.GetBytesToSave(), "edited.GM1");
                Assert.That(reloaded.History.Current.Mines().GetModel(index).Guard.Slots[0]!.Count, Is.EqualTo(12));
            }
        }
    }
}
