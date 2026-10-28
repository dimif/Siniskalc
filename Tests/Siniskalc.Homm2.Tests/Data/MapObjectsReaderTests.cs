namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;

    /// <summary>
    /// How the mines, the shrines, the sphinxes, the current turn and the castles that built something are found in the bytes of a
    /// savegame, on a savegame made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MapObjectsReaderTests
    {
        private readonly MapObjectsReader reader = new();

        private MapObjectChunks Read(MapSavegame save) => this.reader.Read(save.Bytes, save.Layout);

        [Test]
        public void TheCurrentTurnIsTheByteOfTheHumanPlayerWithTheOrderOfTheColoursInItsTag()
        {
            var save = new MapSavegame(6);
            save.Bytes[save.Layout.CurrentHumanStart] = 2;

            var turn = this.Read(save).Turn.Single();

            Assert.That((turn.Name, turn.StartIndex, turn.Length, turn.GetByteAt(0), turn.Tag), Is.EqualTo(("Current turn", save.Layout.CurrentHumanStart, 1, 2, PlayerRecordOrder.Pack([0, 1, 2, 3, 4, 5]))));
        }

        [Test]
        public void TheCastlesThatBuiltSomethingAreNineBytesAtAFixedPlaceAfterTheCastleTable()
        {
            var save = new MapSavegame(6);
            save.Bytes[MapSavegame.MapStart + MapObjectLayout.BuiltToday] = 0b101;

            var built = this.Read(save).BuiltToday.Single();

            Assert.That((built.Name, built.StartIndex, built.Length, built.GetByteAt(0)), Is.EqualTo(("Built this turn", MapSavegame.MapStart + MapObjectLayout.BuiltToday, MapObjectLayout.BuiltTodaySize, 0b101)));
        }

        [Test]
        public void ASavegameWithNoTableOfTilesHasNoMinesAndNoShrinesButStillTheTurnAndTheCastlesThatBuilt()
        {
            var save = new MapSavegame(6, withTiles: false);
            save.Mine(0, 0xFF, (byte)MineKind.GoldMine, 1, 1);

            var chunks = this.Read(save);

            Assert.That((chunks.Mines.Count, chunks.Shrines.Count, chunks.Turn.Count, chunks.BuiltToday.Count), Is.EqualTo((0, 0, 1, 1)));
        }

        [Test]
        public void AMineIsARecordOfSevenBytesWithItsMarkersAndItsTileOnTheMapNamedByItsKindAndTile()
        {
            var save = new MapSavegame(6);
            var at = save.Mine(0, 2, (byte)MineKind.GoldMine, 1, 2);

            var mine = this.Read(save).Mines.Single();

            Assert.That((mine.Name, mine.StartIndex, mine.Length, mine.Index, mine.Tag), Is.EqualTo(("Gold mine at X: 1, Y: 2", at, 7, 0, (int)MineKind.GoldMine)));
            Assert.That(mine.Position, Is.EqualTo(new MapPosition(1, 2)));
        }

        [Test]
        public void ARecordThatIsNoMineIsLeftOutAndTheOthersAreNumberedInOrder()
        {
            var save = new MapSavegame(6);
            save.Mine(0, 0xFF, (byte)MineKind.OreMine, 1, 1);
            save.Mine(1, 6, (byte)MineKind.OreMine, 1, 1);
            save.Mine(2, 0xFF, (byte)MineKind.OreMine, 6, 1);
            save.Mine(3, 0xFF, (byte)MineKind.OreMine, 1, 6);
            save.Mine(4, 0xFF, 7, 1, 1);
            save.Mine(5, 5, (byte)MineKind.GemMine, 4, 4);
            save.Bytes[save.Mine(6, 0xFF, (byte)MineKind.OreMine, 2, 2) + 2] = 0;
            save.Bytes[save.Mine(7, 0xFF, (byte)MineKind.OreMine, 2, 2) + 3] = 0;
            save.Bytes[save.Mine(8, 0xFF, (byte)MineKind.OreMine, 2, 2) + 6] = 0;
            save.Mine(9, 0xFF, (byte)MineKind.Sawmill, 3, 3);

            var mines = this.Read(save).Mines;

            Assert.That(mines.Select(m => (m.Index, m.Name)), Is.EqualTo(new[] { (0, "Ore mine at X: 1, Y: 1"), (1, "Gem mine at X: 4, Y: 4"), (2, "Sawmill at X: 3, Y: 3") }));
        }

        [Test]
        public void TheFlagOfAMineIsAPartOnItsTileWhenTheSlotIsEmptyOrAlreadyHoldsAFlag()
        {
            var save = new MapSavegame(6);
            save.Mine(0, 0xFF, (byte)MineKind.GoldMine, 1, 2);
            save.Mine(1, 0xFF, (byte)MineKind.OreMine, 2, 2);
            var empty = save.Tile(1, 2) + MapObjectLayout.TileSlot;
            var flag = save.Tile(2, 2) + MapObjectLayout.TileSlot;
            new[] { MapObjectLayout.NoClass, MapObjectLayout.NoPicture }.CopyTo(save.Bytes, empty);
            new byte[] { MapObjectLayout.FlagClass, 16 }.CopyTo(save.Bytes, flag);

            var mines = this.Read(save).Mines;

            Assert.That(mines[0].Parts.Select(p => (p.StartIndex, p.Bytes)), Is.EqualTo(new[] { (empty, new[] { MapObjectLayout.NoClass, MapObjectLayout.NoPicture }) }));
            Assert.That(mines[1].Parts.Select(p => p.StartIndex), Is.EqualTo(new[] { flag }));
        }

        [Test]
        public void WhenTheSlotOfTheTileHoldsAnotherObjectTheFlagIsInTheAddonTheTileLinksTo()
        {
            var save = new MapSavegame(6);
            save.Mine(0, 0xFF, (byte)MineKind.GoldMine, 1, 2);
            save.Bytes[save.Tile(1, 2) + MapObjectLayout.TileSlot] = 0x20;
            save.Bytes[save.Tile(1, 2) + MapObjectLayout.TileLink] = 3;
            new[] { MapObjectLayout.NoClass, MapObjectLayout.NoPicture }.CopyTo(save.Bytes, save.AddonAt(3));

            var mine = this.Read(save).Mines.Single();

            Assert.That(mine.Parts.Select(p => p.StartIndex), Is.EqualTo(new[] { save.AddonAt(3) }));
        }

        [Test]
        public void AMineWhoseTileAndAddonHoldOtherObjectsHasNoFlagPartAndAnAlchemistsLabNeverHasOne()
        {
            var save = new MapSavegame(6);
            save.Mine(0, 0xFF, (byte)MineKind.GoldMine, 1, 2);
            save.Mine(1, 0xFF, (byte)MineKind.AlchemistLab, 3, 3);
            save.Bytes[save.Tile(1, 2) + MapObjectLayout.TileSlot] = 0x20;
            save.Bytes[save.Tile(1, 2) + MapObjectLayout.TileLink] = 3;
            save.Bytes[save.AddonAt(3)] = 0x21;
            new[] { MapObjectLayout.NoClass, MapObjectLayout.NoPicture }.CopyTo(save.Bytes, save.Tile(3, 3) + MapObjectLayout.TileSlot);

            var mines = this.Read(save).Mines;

            Assert.That(mines.Select(m => m.Parts.Count), Is.EqualTo(new[] { 0, 0 }));
        }

        [Test]
        public void TheFlagOfASawmillIsOnTheTileUpAndToTheRightAndNoneAtTheEdgeOfTheMap()
        {
            var save = new MapSavegame(6);
            save.Mine(0, 0xFF, (byte)MineKind.Sawmill, 2, 3);
            save.Mine(1, 0xFF, (byte)MineKind.Sawmill, 5, 3);
            save.Mine(2, 0xFF, (byte)MineKind.Sawmill, 2, 0);
            var upRight = save.Tile(3, 2) + MapObjectLayout.TileSlot;
            new[] { MapObjectLayout.NoClass, MapObjectLayout.NoPicture }.CopyTo(save.Bytes, upRight);

            var mines = this.Read(save).Mines;

            Assert.That(mines.Select(m => m.Parts.Select(p => p.StartIndex).ToList()), Is.EqualTo(new[] { new List<int> { upRight }, [], [] }));
        }

        [Test]
        public void AShrineIsFoundAtEachTileWithTheObjectOfOneOfTheThreeCirclesAndHoldsTheTwoBytesOfItsQuantity()
        {
            var save = new MapSavegame(6);
            save.Object(1, 0, MapObjectLayout.ShrineFirstCircle);
            save.Object(4, 1, MapObjectLayout.ShrineSecondCircle);
            save.Object(2, 5, MapObjectLayout.ShrineThirdCircle);
            save.Bytes[save.Tile(1, 0) + MapObjectLayout.TileQuantity] = 0x58;

            var shrines = this.Read(save).Shrines;

            Assert.That(shrines.Select(s => (s.Name, s.Tag, s.Index, s.StartIndex)), Is.EqualTo(new[]
            {
                ("Shrine at 1, 0", 1, 0, save.Tile(1, 0) + 4),
                ("Shrine at 4, 1", 2, 1, save.Tile(4, 1) + 4),
                ("Shrine at 2, 5", 3, 2, save.Tile(2, 5) + 4),
            }));
            Assert.That(shrines[0].Length, Is.EqualTo(2));
            Assert.That(shrines[0].GetByteAt(0), Is.EqualTo(0x58));
        }

        [Test]
        public void AShrineWithAHeroOnItIsHiddenAndKeepsItsCircleInTheTag()
        {
            var save = new MapSavegame(6);
            save.Hero(0, 3, 1, MapObjectLayout.ShrineSecondCircle);

            var shrine = this.Read(save).Shrines.Single();

            Assert.That(shrine.Tag, Is.EqualTo(2 | MapObjectsReader.HiddenShrine));
        }

        [Test]
        public void ASphinxIsABlockWithTheMarkerOfABlockAndItsLengthAndTheBlocksAreNumberedFromOne()
        {
            var save = new MapSavegame(6);
            var first = save.Sphinx(0);
            var second = save.Sphinx(1, b => b[MapObjectLayout.SphinxActive] = 0, 300);

            var sphinxes = this.Read(save).Sphinxes;

            Assert.That(sphinxes.Select(s => (s.Name, s.StartIndex, s.Length, s.Index)), Is.EqualTo(new[] { ("Sphinx 1", first, 236, 0), ("Sphinx 2", second, 300, 1) }));
        }

        [Test]
        public void ABlockThatIsNoSphinxIsLeftOut()
        {
            var save = new MapSavegame(6);
            save.Sphinx(0, b => b[MapObjectLayout.SphinxActive] = 2);
            save.Sphinx(1, b => b[MapObjectLayout.SphinxAnswerCount] = 0);
            save.Sphinx(2, b => b[MapObjectLayout.SphinxAnswerCount] = 9);
            save.Sphinx(3, b => b[MapObjectLayout.SphinxAnswers] = 0x01);
            save.Sphinx(4, b => b[MapObjectLayout.SphinxRiddle] = 0);
            save.Sphinx(5, length: 100);
            var real = save.Sphinx(6);

            var sphinxes = this.Read(save).Sphinxes;

            Assert.That(sphinxes.Select(s => s.StartIndex), Is.EqualTo(new[] { real }));
        }

        [Test]
        public void ASphinxHasItsTileWhenTheMapHasAsManySphinxTilesAsBlocksAndNoneOtherwise()
        {
            var matching = new MapSavegame(6);
            matching.Sphinx(0);
            matching.Sphinx(1);
            matching.Object(4, 1, MapObjectLayout.Sphinx);
            matching.Object(1, 3, MapObjectLayout.Sphinx);

            var mismatched = new MapSavegame(6);
            mismatched.Sphinx(0);
            mismatched.Sphinx(1);
            mismatched.Object(4, 1, MapObjectLayout.Sphinx);

            Assert.That(this.Read(matching).Sphinxes.Select(s => s.Position), Is.EqualTo(new[] { new MapPosition(4, 1), new MapPosition(1, 3) }));
            Assert.That(this.Read(mismatched).Sphinxes.Select(s => s.Position), Is.All.Null);
        }

        [Test]
        public void WithNoTableOfTilesTheSphinxesAreStillFoundWithNoTile()
        {
            var save = new MapSavegame(6, withTiles: false);
            save.Sphinx(0);

            var sphinx = this.Read(save).Sphinxes.Single();

            Assert.That(sphinx.Position, Is.Null);
        }
    }
}
