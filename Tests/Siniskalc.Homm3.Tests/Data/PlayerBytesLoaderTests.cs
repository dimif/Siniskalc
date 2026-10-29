using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// How the table of players is found in the bytes of a savegame, on savegames made by hand for both layouts: a table is a run of
    /// records of the size of a record, each with the marker of a player and a colour byte that goes up.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class PlayerBytesLoaderTests
    {
        private const int TableStart = 700;

        private readonly PlayerBytesLoader loader = new();

        private static IEnumerable<PlayerLayout> Layouts => PlayerRecords.Layouts;

        /// <summary>A savegame of zeros with a table of players: the colour bytes of the records in order, and what to change after.</summary>
        private static byte[] Savegame(PlayerLayout layout, int[] colorBytes, Action<byte[], int>? change = null, int tableStart = TableStart)
        {
            var bytes = new byte[tableStart + (colorBytes.Length * layout.Stride) + 400];
            for (var i = 0; i < colorBytes.Length; i++)
            {
                var start = tableStart + (i * layout.Stride);
                layout.Marker.CopyTo(bytes, start + layout.MarkerOffset);
                bytes[start + layout.ColorOffset] = (byte)colorBytes[i];
            }

            for (var i = 0; i < colorBytes.Length; i++)
            {
                change?.Invoke(bytes, tableStart + (i * layout.Stride));
            }

            return bytes;
        }

        private static readonly int[] EightColors = [1, 2, 3, 4, 5, 6, 7, 8];

        [Test]
        public void AVersionWhoseLayoutIsNotKnownHasNoTable()
        {
            var bytes = Savegame(PlayerLayout.ShadowOfDeath, EightColors);

            Assert.That(this.loader.LoadPlayerChunks(bytes, GameVersion.Unknown), Is.Empty);
        }

        [TestCaseSource(nameof(Layouts))]
        public void ATableOfEightPlayersGivesAChunkForEachPlayerInPlayWithItsColourItsPlaceAndTheSizeOfARecord(PlayerLayout layout)
        {
            var version = layout == PlayerLayout.HornOfTheAbyss ? GameVersion.HornOfTheAbyss : GameVersion.ShadowOfDeath;
            var bytes = Savegame(layout, EightColors, (b, start) => BitConverter.GetBytes(1000).CopyTo(b, start + PlayerLayout.ResourcesOffset));

            var chunks = this.loader.LoadPlayerChunks(bytes, version);

            Assert.That(chunks.Select(c => c.Color), Is.EqualTo(new[] { PlayerColor.Red, PlayerColor.Blue, PlayerColor.Tan, PlayerColor.Green, PlayerColor.Orange, PlayerColor.Purple, PlayerColor.Teal, PlayerColor.Pink }));
            Assert.That(chunks.Select(c => c.StartIndex), Is.EqualTo(Enumerable.Range(0, 8).Select(i => TableStart + (i * layout.Stride))));
            Assert.That(chunks.Select(c => c.Length), Has.All.EqualTo(layout.Stride));
            Assert.That(chunks.Select(c => c.Layout), Has.All.SameAs(layout));
        }

        [Test]
        public void OnlyThePlayersInPlayAreKeptAPlayerWithResourcesHeroesOrTownsBeingInPlay()
        {
            var layout = PlayerLayout.ShadowOfDeath;
            var bytes = Savegame(layout, EightColors);
            BitConverter.GetBytes(5).CopyTo(bytes, TableStart + PlayerLayout.ResourcesOffset);
            bytes[TableStart + (2 * layout.Stride) + PlayerLayout.HeroCount] = 1;
            bytes[TableStart + (5 * layout.Stride) + PlayerLayout.TownCount] = 1;

            var chunks = this.loader.LoadPlayerChunks(bytes, GameVersion.ShadowOfDeath);

            Assert.That(chunks.Select(c => c.Color), Is.EqualTo(new[] { PlayerColor.Red, PlayerColor.Tan, PlayerColor.Purple }));
        }

        [Test]
        public void WhenNoPlayerIsInPlayEveryChunkOfTheTableIsKept()
        {
            var bytes = Savegame(PlayerLayout.ShadowOfDeath, EightColors);

            Assert.That(this.loader.LoadPlayerChunks(bytes, GameVersion.ShadowOfDeath), Has.Count.EqualTo(8));
        }

        [Test]
        public void ARecordWithNoColourByteTakesTheColourAfterThePreviousOne()
        {
            var layout = PlayerLayout.ShadowOfDeath;
            var bytes = Savegame(layout, [1, 2, 0, 4, 5, 6, 7, 8], (b, start) => BitConverter.GetBytes(1000).CopyTo(b, start + PlayerLayout.ResourcesOffset));

            var chunks = this.loader.LoadPlayerChunks(bytes, GameVersion.ShadowOfDeath);

            Assert.That(chunks.Select(c => c.Color), Is.EqualTo(new[] { PlayerColor.Red, PlayerColor.Blue, PlayerColor.Tan, PlayerColor.Green, PlayerColor.Orange, PlayerColor.Purple, PlayerColor.Teal, PlayerColor.Pink }));
        }

        [Test]
        public void ARunOfFewerThanTwoRecordsIsNoTable()
        {
            var bytes = Savegame(PlayerLayout.ShadowOfDeath, [3]);

            Assert.That(this.loader.LoadPlayerChunks(bytes, GameVersion.ShadowOfDeath), Is.Empty);
        }

        [Test]
        public void TheLongestRunIsTheTableWhenARunOfRecordsAppearsElsewhere()
        {
            var layout = PlayerLayout.ShadowOfDeath;
            var bytes = new byte[TableStart + (14 * layout.Stride) + 400];
            for (var i = 0; i < 2; i++)
            {
                WritePlayer(bytes, layout, TableStart + (i * layout.Stride), i + 1);
            }

            var second = TableStart + (4 * layout.Stride);
            for (var i = 0; i < 6; i++)
            {
                WritePlayer(bytes, layout, second + (i * layout.Stride), i + 1);
            }

            var chunks = this.loader.LoadPlayerChunks(bytes, GameVersion.ShadowOfDeath);

            Assert.That(chunks, Has.Count.EqualTo(6));
            Assert.That(chunks[0].StartIndex, Is.EqualTo(second));
        }

        private static void WritePlayer(byte[] bytes, PlayerLayout layout, int start, int color)
        {
            layout.Marker.CopyTo(bytes, start + layout.MarkerOffset);
            bytes[start + layout.ColorOffset] = (byte)color;
        }

        [Test]
        public void ARecordWithANegativeResourceIsNoPlayerSoTheRunEndsThereAndTheLongestRunWins()
        {
            var layout = PlayerLayout.ShadowOfDeath;
            var bytes = Savegame(layout, EightColors, (b, start) => BitConverter.GetBytes(10).CopyTo(b, start + PlayerLayout.ResourcesOffset));
            BitConverter.GetBytes(-1).CopyTo(bytes, TableStart + (5 * layout.Stride) + PlayerLayout.ResourcesOffset);

            var chunks = this.loader.LoadPlayerChunks(bytes, GameVersion.ShadowOfDeath);

            Assert.That(chunks.Select(c => c.Color), Is.EqualTo(new[] { PlayerColor.Red, PlayerColor.Blue, PlayerColor.Tan, PlayerColor.Green, PlayerColor.Orange }), "five records before the broken one beat the two after it");
        }

        [TestCaseSource(nameof(Layouts))]
        public void AChunkThatStartsRightAfterTheTableLeavesTheLastPlayerWhole(PlayerLayout layout)
        {
            var version = layout == PlayerLayout.HornOfTheAbyss ? GameVersion.HornOfTheAbyss : GameVersion.ShadowOfDeath;
            var bytes = Savegame(layout, EightColors, (b, start) => BitConverter.GetBytes(10).CopyTo(b, start + PlayerLayout.ResourcesOffset));
            var lastStart = TableStart + (7 * layout.Stride);
            var castle = new RawBytesChunk("Castle", new byte[50], lastStart + layout.Stride);

            var chunks = this.loader.LoadPlayerChunks(bytes, version, [castle]);

            Assert.That((chunks.Count, chunks[7].StartIndex, chunks[7].Length), Is.EqualTo((8, lastStart, layout.Stride)));
        }

        [Test]
        public void ALastPlayerWhoseColourByteWouldBeCutOffByAnotherChunkIsLeftOut()
        {
            var layout = PlayerLayout.ShadowOfDeath;
            var bytes = Savegame(layout, EightColors, (b, start) => BitConverter.GetBytes(10).CopyTo(b, start + PlayerLayout.ResourcesOffset));
            var lastStart = TableStart + (7 * layout.Stride);
            var castle = new RawBytesChunk("Castle", new byte[50], lastStart + 20);

            var chunks = this.loader.LoadPlayerChunks(bytes, GameVersion.ShadowOfDeath, [castle]);

            Assert.That(chunks.Select(c => c.Color), Is.EqualTo(new[] { PlayerColor.Red, PlayerColor.Blue, PlayerColor.Tan, PlayerColor.Green, PlayerColor.Orange, PlayerColor.Purple, PlayerColor.Teal }));
        }
    }
}
