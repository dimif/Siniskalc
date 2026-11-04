using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.Integration.Data
{
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class DwellingFindingTests
    {
        private const string Root = "TestData\\Hota\\MineFinding";

        private static IEnumerable<string> SmallSaves => Directory.GetFiles(Root, "*.GM1", SearchOption.AllDirectories);

        private static IEnumerable<string> AllSaves => Directory.GetFiles("TestData", "*.GM?", SearchOption.AllDirectories);

        [TestCase("NoMinesOneDwelling", 1)]
        [TestCase("OneMineNoDwellings", 0)]
        [TestCase("OneMineOneDwelling", 1)]
        [TestCase("OneMineTwoDwellings", 2)]
        [TestCase("TwoMinesTwoDwellings", 2)]
        [TestCase("TwoMinesThreeDwellings", 3)]
        public void EverySmallMapCaptureSaveHasExactlyItsDwellingsRegardlessOfItsMineCount(string folder, int expected)
        {
            foreach (var file in Directory.GetFiles(Path.Combine(Root, folder), "*.GM1"))
            {
                Assert.That(SavedGames.Get(file).History.Current.Dwellings(), Has.Count.EqualTo(expected), file);
            }
        }

        [Test]
        public void TheNoMineMapFindsItsDevilDwellingAndItsCaptureWithoutHiring()
        {
            var folder = Path.Combine(Root, "NoMinesOneDwelling");
            var before = SavedGames.Get(Path.Combine(folder, "no_mines_one_dwelling_00_baseline.GM1")).History.Current;
            var after = SavedGames.Get(Path.Combine(folder, "no_mines_one_dwelling_01_red_captures_dwelling.GM1")).History.Current;
            Assert.That(before.Mines(), Is.Empty);
            Assert.That(before.Dwellings(), Has.Count.EqualTo(1));
            var dwelling = before.Dwellings().GetModel(0);
            Assert.That((dwelling.Creature?.Code, dwelling.Owner, dwelling.Stock, dwelling.Position),
                Is.EqualTo((54, PlayerColor.Neutral, 1L, new MapPosition(10, 10))));
            Assert.That(after.Dwellings().GetModel(0), Is.EqualTo(dwelling with { Owner = PlayerColor.Red }));
            Assert.That(before.Dwellings()[0].StartIndex, Is.EqualTo(240148));
        }

        [Test]
        public void TheThreeDwellingsHaveTheirOwnCreaturesStockAndTilesAndAreCapturedOneAtATime()
        {
            const string folder = "TwoMinesThreeDwellings";
            var directory = Path.Combine(Root, folder);
            var baseline = SavedGames.Get(Path.Combine(directory, "two_mines_three_dwellings_00_baseline.GM1")).History.Current.Dwellings();
            Assert.That(baseline.GetModels().Select(d => (d.Creature?.Code, d.Stock, d.Position)), Is.EqualTo(new[]
            {
                (54, 1L, new MapPosition(11, 8)),
                (198, 1L, new MapPosition(14, 8)),
                (176, 5L, new MapPosition(16, 8)),
            }));
            Assert.That(baseline.Select(c => c.StartIndex), Is.EqualTo(new[] { 240607, 240682, 240757 }));
            foreach (var name in SeriesSaves.Names(Path.Combine("Hota", "MineFinding", folder), "two_mines_three_dwellings"))
            {
                var step = SeriesSaves.Number(name);
                var dwellings = SavedGames.Get(Path.Combine(directory, name + ".GM1")).History.Current.Dwellings();
                for (var index = 0; index < 3; index++)
                {
                    var owner = step >= index + 3 ? PlayerColor.Red : PlayerColor.Neutral;
                    Assert.That(dwellings.GetModel(index), Is.EqualTo(baseline.GetModel(index) with { Owner = owner }), name);
                }
            }
        }

        [Test]
        public void TheArtsMapFindsAllFiveDwellingsWithoutIncludingTheObeliskMarker()
        {
            foreach (var file in Directory.GetFiles("TestData\\Hota\\Arts", "*.GM1"))
            {
                var document = SavedGames.Get(file).History.Current;
                var dwellings = document.Dwellings();
                Assert.That(dwellings, Has.Count.EqualTo(5), file);
                Assert.That(dwellings.GetModels().Select(d => (d.Creature?.Code, d.Position)), Is.EqualTo(new[]
                {
                    (94, new MapPosition(53, 33)),
                    (36, new MapPosition(45, 40)),
                    (16, new MapPosition(12, 70)),
                    (14, new MapPosition(22, 58)),
                    (36, new MapPosition(47, 42)),
                }), file);
                Assert.That(dwellings[0].StartIndex, Is.EqualTo(document.Mines()[^1].End + 2));
                Assert.That(dwellings[^1].End, Is.EqualTo(document.Obelisks()[0].StartIndex), "the obelisk marker is not a dwelling byte");
            }
        }

        [Test]
        public void EditingTheLastArtsDwellingAndAnObeliskWritesDistinctBytesAndPreservesTheirSharedBoundary()
        {
            const string file = "TestData\\Hota\\Arts\\arts_00_before_arts.GM1";
            var game = SavedGames.Get(file);
            var history = game.History;
            var before = game.GetBytesToSave();
            var last = history.Current.Dwellings()[^1];
            var obelisks = history.Current.Obelisks()[0];
            history.CreateDwellingSession(4).Update(d => d with { Stock = 7 });
            history.CreateObeliskSession().Update(v => v.WithVisited(0, PlayerColor.Red, true));

            var written = game.GetBytesToSave();
            Assert.That(ByteDiff.Of(before, written).Select(d => d.Offset), Is.EqualTo(new[] { last.StartIndex + 7, obelisks.StartIndex + 4 }));
            Assert.That(written[obelisks.StartIndex], Is.EqualTo(255));
            var reloaded = TestServices.GameService.LoadGameFromBytes(written, "arts_edited.GM1").History.Current;
            Assert.That(reloaded.Dwellings().GetModel(4).Stock, Is.EqualTo(7));
            Assert.That(reloaded.Obelisks().GetModel()!.VisitedCount(PlayerColor.Red), Is.EqualTo(1));
            foreach (var change in history.GetPendingChanges().ToList())
            {
                change.Revert(history);
            }

            Assert.That(game.GetBytesToSave(), Is.EqualTo(before));
        }

        [TestCaseSource(nameof(SmallSaves))]
        public void EditingEachSmallMapDwellingWritesOnlyItsOwnerAndStockAndCanBeUndoneRedoneAndReverted(string file)
        {
            var count = SavedGames.Get(file).History.Current.Dwellings().Count;
            for (var index = 0; index < count; index++)
            {
                var game = SavedGames.Get(file);
                var history = game.History;
                var before = game.GetBytesToSave();
                var chunk = history.Current.Dwellings()[index];
                var model = history.Current.Dwellings().GetModel(index);
                history.CreateDwellingSession(index).Update(d => d with { Owner = PlayerColor.Blue, Stock = 7 });
                Assert.That(ByteDiff.Of(before, game.GetBytesToSave()), Is.EqualTo(new[]
                {
                    (chunk.StartIndex, (byte)model.Owner.Code, (byte)1),
                    (chunk.StartIndex + 7, (byte)model.Stock, (byte)7),
                }));
                var reloaded = TestServices.GameService.LoadGameFromBytes(game.GetBytesToSave(), "dwelling_edited.GM1");
                Assert.That(reloaded.History.Current.Dwellings().GetModel(index), Is.EqualTo(model with { Owner = PlayerColor.Blue, Stock = 7 }));
                history.Undo();
                Assert.That(game.GetBytesToSave(), Is.EqualTo(before));
                history.Redo();
                Assert.That(history.Current.Dwellings().GetModel(index).Stock, Is.EqualTo(7));
                foreach (var change in history.GetPendingChanges().ToList())
                {
                    change.Revert(history);
                }

                Assert.That(game.GetBytesToSave(), Is.EqualTo(before));
            }
        }

        [TestCaseSource(nameof(AllSaves))]
        public void EverySaveAndEveryLocatedDwellingRoundTripsWithoutOverlappingOrChangingBytes(string file)
        {
            var game = SavedGames.Get(file);
            var bytes = TestServices.SavefileCodec.Read(file);
            var table = new MapObjectTablesLocator().Locate(bytes, game.History.Current.Catalog().Version);
            int expected;
            if (Path.GetDirectoryName(file) == Path.Combine("TestData", "Hota", "MerchantStock")
                || Path.GetDirectoryName(file) == Path.Combine("TestData", "Hota", "DragonUtopias")
                || Path.GetDirectoryName(file) == Path.Combine("TestData", "Hota", "CreatureBanks"))
            {
                Assert.That(table, Is.Null, "the merchant/creature-bank companion maps have no mine or dwelling definitions");
                Assert.That(game.History.Current.Mines(), Is.Empty);
                expected = 0;
            }
            else
            {
                Assert.That(table, Is.Not.Null, "the other maps in this corpus have a nonempty owner/guard or generator table");
                expected = Enumerable.Range(0, table!.GeneratorCount)
                    .Count(i => bytes[table.GeneratorStart + (i * 75) + 1] == 17);
            }

            var dwellings = game.History.Current.Dwellings();
            Assert.That(dwellings, Has.Count.EqualTo(expected), "no validated single-creature dwelling is silently lost");
            foreach (var chunk in dwellings)
            {
                Assert.That(dwellings.Serializer.Serialize(chunk, dwellings.Deserializer.Deserialize(chunk)).ToArray(), Is.EqualTo(chunk.ToArray()));
            }

            Assert.That(game.GetBytesToSave(), Is.EqualTo(bytes));
        }
    }
}
