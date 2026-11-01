using Siniskalc.Homm3.Entities;
using System.Text;
using Siniskalc.Homm3.Data;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// How the castles are found in the bytes of a savegame, on savegames made by hand: castles are in no table, so each is found by
    /// what it looks like (a prefix, the length of the name, a printable name, and a valid owner, faction, garrison and spell counts).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class CastleBytesLoaderTests
    {
        private const int NameOffset = 71;
        private const int NameLengthOffset = 69;
        private const int FullTail = 233;
        private const int MinimumTail = 113;

        private readonly CastleBytesLoader loader = new(Encoding.Latin1, TestCatalog.For(GameVersion.HornOfTheAbyss));

        /// <summary>A savegame of zeros with one castle record at <paramref name="start"/>, then the changes.</summary>
        private static byte[] Savegame(string name, int start = 100, int after = 400, Action<byte[], int, int>? change = null)
        {
            var bytes = new byte[start + NameOffset + name.Length + after];
            BitConverter.GetBytes((ushort)name.Length).CopyTo(bytes, start + NameLengthOffset);
            Encoding.Latin1.GetBytes(name).CopyTo(bytes, start + NameOffset);
            change?.Invoke(bytes, start, start + NameOffset + name.Length);
            return bytes;
        }

        [Test]
        public void BytesWithNoCastleHaveNoChunks()
        {
            Assert.That(this.loader.LoadCastleChunks(new byte[1000]), Is.Empty);
            Assert.That(this.loader.LoadCastleChunks([]), Is.Empty);
        }

        [Test]
        public void ACastleIsAChunkNamedAfterItFromItsFirstByteToTheEndOfTheSpellLists()
        {
            var bytes = Savegame("Stormwatch");

            var chunk = this.loader.LoadCastleChunks(bytes).Single();

            Assert.That((chunk.Name, chunk.StartIndex, chunk.Length), Is.EqualTo(("Stormwatch", 100, NameOffset + 10 + FullTail)));
            Assert.That(chunk.ToArray(), Is.EqualTo(bytes.AsSpan(100, NameOffset + 10 + FullTail).ToArray()));
        }

        [Test]
        public void ACastleAtTheVeryStartOfTheSavegameIsFound()
        {
            var chunk = this.loader.LoadCastleChunks(Savegame("Stormwatch", start: 0)).Single();

            Assert.That(chunk.StartIndex, Is.EqualTo(0));
        }

        [Test]
        public void SeveralCastlesAreFoundInTheOrderOfTheirBytes()
        {
            var first = Savegame("Alpha", start: 0, after: 300);
            var second = Savegame("Beta", start: 0, after: 400);
            var bytes = first.Concat(second).ToArray();

            var chunks = this.loader.LoadCastleChunks(bytes);

            Assert.That(chunks.Select(c => c.Name), Is.EqualTo(new[] { "Alpha", "Beta" }));
            Assert.That(chunks[1].StartIndex, Is.EqualTo(first.Length));
        }

        [Test]
        public void WhatLooksLikeACastleInsideTheSpellListsOfAnotherIsNotFoundAgainstTheOneThatHoldsIt()
        {
            var bytes = Savegame("Stormwatch", after: 600, change: (b, _, _) =>
            {
                BitConverter.GetBytes((ushort)5).CopyTo(b, 300 + NameLengthOffset);
                Encoding.Latin1.GetBytes("Inner").CopyTo(b, 300 + NameOffset);
            });

            var chunks = this.loader.LoadCastleChunks(bytes);

            Assert.That(chunks.Select(c => c.Name), Is.EqualTo(new[] { "Stormwatch" }));
        }

        [Test]
        public void ANameWithACharacterAboveSevenEIsNoCastle()
        {
            var bytes = Savegame("Café");

            Assert.That(this.loader.LoadCastleChunks(bytes), Is.Empty);
        }

        [Test]
        public void ASavegameThatStopsBeforeTheSpellListsGivesAShorterChunk()
        {
            var bytes = Savegame("Stormwatch", after: MinimumTail);

            var chunk = this.loader.LoadCastleChunks(bytes).Single();

            Assert.That(chunk.Length, Is.EqualTo(bytes.Length - 100));
        }

        [Test]
        public void ASavegameThatStopsBeforeTheMasksHasNoCastle()
        {
            Assert.That(this.loader.LoadCastleChunks(Savegame("Stormwatch", after: MinimumTail - 1)), Is.Empty);
        }

        [TestCase(7, true)]
        [TestCase(0xFF, true)]
        [TestCase(8, false)]
        [TestCase(0xFE, false)]
        public void TheOwnerIsAColourOrNone(int owner, bool found)
        {
            var bytes = Savegame("Stormwatch", change: (b, start, _) => b[start + CastleLayout.Owner] = (byte)owner);

            Assert.That(this.loader.LoadCastleChunks(bytes), found ? Has.Count.EqualTo(1) : Is.Empty);
        }

        [TestCase(1, true)]
        [TestCase(2, false)]
        public void BuiltThisTurnIsZeroOrOne(int value, bool found)
        {
            var bytes = Savegame("Stormwatch", change: (b, start, _) => b[start + CastleLayout.BuiltThisTurn] = (byte)value);

            Assert.That(this.loader.LoadCastleChunks(bytes), found ? Has.Count.EqualTo(1) : Is.Empty);
        }

        [TestCase(11, true)]
        [TestCase(12, false)]
        [TestCase(99, false)]
        public void TheFactionIsOneOfTheCodesOfTheFactionsOfTheGame(int faction, bool found)
        {
            var bytes = Savegame("Stormwatch", change: (b, start, _) => b[start + CastleLayout.Faction] = (byte)faction);

            Assert.That(this.loader.LoadCastleChunks(bytes), found ? Has.Count.EqualTo(1) : Is.Empty);
        }

        [TestCase(5, true)]
        [TestCase(6, false)]
        public void TheMageGuildLevelIsFiveAtTheMost(int level, bool found)
        {
            var bytes = Savegame("Stormwatch", change: (b, start, _) => b[start + CastleLayout.MageGuildLevel] = (byte)level);

            Assert.That(this.loader.LoadCastleChunks(bytes), found ? Has.Count.EqualTo(1) : Is.Empty);
        }

        [Test]
        public void TheByteAfterTheMageGuildLevelMustBeZero()
        {
            var bytes = Savegame("Stormwatch", change: (b, start, _) => b[start + CastleLayout.NameLength - 1] = 1);

            Assert.That(this.loader.LoadCastleChunks(bytes), Is.Empty);
        }

        [Test]
        public void TheFirstTwoBytesOfThePrefixAreAnything()
        {
            var bytes = Savegame("Stormwatch", change: (b, start, _) =>
            {
                b[start + CastleLayout.NamePrefix] = 0xAB;
                b[start + CastleLayout.NamePrefix + 1] = 0xCD;
            });

            Assert.That(this.loader.LoadCastleChunks(bytes), Has.Count.EqualTo(1));
        }

        [TestCase(48, true)]
        [TestCase(49, false)]
        [TestCase(1, true)]
        public void ANameOfOneToFortyEightCharactersIsACastle(int length, bool found)
        {
            Assert.That(this.loader.LoadCastleChunks(Savegame(new string('a', length))), found ? Has.Count.EqualTo(1) : Is.Empty);
        }

        [Test]
        public void ANameOfNoCharactersIsNoCastle()
        {
            Assert.That(this.loader.LoadCastleChunks(Savegame(string.Empty)), Is.Empty);
        }

        [TestCase(0x20, true)]
        [TestCase(0x7E, true)]
        [TestCase(0x1F, false)]
        [TestCase(0x7F, false)]
        public void EveryCharacterOfTheNameIsPrintable(int character, bool found)
        {
            var bytes = Savegame("Stormwatch", change: (b, start, _) => b[start + NameOffset + 4] = (byte)character);

            Assert.That(this.loader.LoadCastleChunks(bytes), found ? Has.Count.EqualTo(1) : Is.Empty);
        }

        [Test]
        public void AGarrisonAllowsAnAbsentCreatureAndEveryRepresentableCreatureCode()
        {
            var bytes = Savegame("Stormwatch", change: (b, start, _) =>
            {
                BitConverter.GetBytes(-1).CopyTo(b, start + CastleLayout.GarrisonTypes);
                BitConverter.GetBytes(255).CopyTo(b, start + CastleLayout.GarrisonTypes + 4);
            });

            Assert.That(this.loader.LoadCastleChunks(bytes), Has.Count.EqualTo(1));
        }

        [TestCase(256)]
        [TestCase(-2)]
        public void ACreatureTypeOutsideWhatTheGameHasIsNoCastle(int type)
        {
            var bytes = Savegame("Stormwatch", change: (b, start, _) => BitConverter.GetBytes(type).CopyTo(b, start + CastleLayout.GarrisonTypes + 12));

            Assert.That(this.loader.LoadCastleChunks(bytes), Is.Empty);
        }

        [Test]
        public void ANegativeCreatureCountIsNoCastle()
        {
            var bytes = Savegame("Stormwatch", change: (b, start, _) => BitConverter.GetBytes(-1).CopyTo(b, start + CastleLayout.GarrisonCounts + 24));

            Assert.That(this.loader.LoadCastleChunks(bytes), Is.Empty);
        }

        [TestCase(8, true)]
        [TestCase(9, false)]
        public void TheGuildHasEightSpellsOfALevelAtTheMost(int count, bool found)
        {
            var bytes = Savegame("Stormwatch", change: (b, _, nameEnd) => b[nameEnd + CastleLayout.SpellCountsFromNameEnd + 4] = (byte)count);

            Assert.That(this.loader.LoadCastleChunks(bytes), found ? Has.Count.EqualTo(1) : Is.Empty);
        }
    }
}
