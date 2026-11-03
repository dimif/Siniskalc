using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MapObjectTablesLocatorTests
    {
        private readonly MapObjectTablesLocator locator = new();

        private static byte[] Generator()
        {
            var bytes = new byte[75];
            bytes[0] = 255;
            bytes[1] = 17;
            bytes[3] = 54;
            bytes[7] = 1;
            bytes[15] = 10;
            bytes[16] = 10;
            for (var slot = 0; slot < 7; slot++)
            {
                BitConverter.GetBytes(uint.MaxValue).CopyTo(bytes, 18 + (slot * 4));
            }

            return bytes;
        }

        private static byte[] OwnerGuard(byte kind)
        {
            var bytes = new byte[62];
            bytes[0] = 255;
            bytes[1] = kind;
            bytes[59] = 3;
            bytes[60] = 8;
            for (var slot = 0; slot < 7; slot++)
            {
                BitConverter.GetBytes(uint.MaxValue).CopyTo(bytes, 3 + (slot * 4));
            }

            return bytes;
        }

        private static byte[] EmptyOwners(params byte[][] generators) =>
            new byte[4].Concat(BitConverter.GetBytes((ushort)generators.Length)).Concat(generators.SelectMany(r => r)).ToArray();

        [TestCase(GameVersion.HornOfTheAbyss, 4)]
        [TestCase(GameVersion.ShadowOfDeath, 1)]
        public void AnExplicitEmptyOwnerGuardTableAllowsOneDwellingWithoutAnyMines(GameVersion version, int countSize)
        {
            var bytes = new byte[countSize].Concat(new byte[] { 1, 0 }).Concat(Generator()).ToArray();
            Assert.That(this.locator.Locate(bytes, version), Is.EqualTo(new MapObjectTables(countSize, 0, countSize + 2, 1)));
        }

        [Test]
        public void TheFullOwnerGuardCountLocatesDwellingsAfterTrailingNonMineObjects()
        {
            var bytes = new byte[] { 2, 0, 0, 0 }.Concat(OwnerGuard(6)).Concat(OwnerGuard(100))
                .Concat(new byte[] { 1, 0 }).Concat(Generator()).ToArray();
            Assert.That(this.locator.Locate(bytes, GameVersion.HornOfTheAbyss), Is.EqualTo(new MapObjectTables(4, 2, 130, 1)));
        }

        [Test]
        public void ATableOfOnlyNonMineOwnerGuardObjectsStillLocatesTheDwellings()
        {
            var bytes = new byte[] { 1, 0, 0, 0 }.Concat(OwnerGuard(100)).Concat(new byte[] { 1, 0 }).Concat(Generator()).ToArray();
            Assert.That(this.locator.Locate(bytes, GameVersion.HornOfTheAbyss), Is.EqualTo(new MapObjectTables(4, 1, 68, 1)));
        }

        [TestCase(17)]
        [TestCase(18)]
        [TestCase(19)]
        [TestCase(20)]
        public void MixedGeneratorKindsAreValidatedWithoutEndingTheTable(byte kind)
        {
            var middle = Generator();
            middle[1] = kind;
            var bytes = EmptyOwners(Generator(), middle, Generator());
            Assert.That(this.locator.Locate(bytes, GameVersion.HornOfTheAbyss), Is.EqualTo(new MapObjectTables(4, 0, 6, 3)));
        }

        [Test]
        public void CompetingEmptyOwnerTablesAreRefusedInsteadOfPickingTheFirst()
        {
            var bytes = EmptyOwners(Generator()).Concat(new byte[11]).Concat(EmptyOwners(Generator())).ToArray();
            Assert.That(this.locator.Locate(bytes, GameVersion.HornOfTheAbyss), Is.Null);
        }

        [Test]
        public void ASeparateEmptyOwnerTableCompetesWithANonemptyOwnerTable()
        {
            var bytes = new byte[] { 1, 0, 0, 0 }.Concat(OwnerGuard(6)).Concat(new byte[] { 0, 0 })
                .Concat(new byte[11]).Concat(EmptyOwners(Generator())).ToArray();
            Assert.That(this.locator.Locate(bytes, GameVersion.HornOfTheAbyss), Is.Null);
        }

        [Test]
        public void ALooseGeneratorWithoutItsCountsIsNotExposed()
        {
            Assert.That(this.locator.Locate(Generator(), GameVersion.HornOfTheAbyss), Is.Null);
            Assert.That(this.locator.Locate(new byte[] { 1, 0 }.Concat(Generator()).ToArray(), GameVersion.HornOfTheAbyss), Is.Null);
            Assert.That(this.locator.Locate(new byte[1000], GameVersion.HornOfTheAbyss), Is.Null);
            Assert.That(this.locator.Locate([], GameVersion.HornOfTheAbyss), Is.Null);
        }

        [Test]
        public void TruncatedAndOversizedGeneratorCountsAreRefused()
        {
            var bytes = EmptyOwners(Generator(), Generator());
            Assert.That(this.locator.Locate(bytes[..^2], GameVersion.HornOfTheAbyss), Is.Null);
            bytes[4] = 255;
            bytes[5] = 255;
            Assert.That(this.locator.Locate(bytes, GameVersion.HornOfTheAbyss), Is.Null);
        }

        [TestCase(0, 8)]
        [TestCase(1, 16)]
        [TestCase(1, 21)]
        [TestCase(15, 252)]
        [TestCase(16, 252)]
        [TestCase(17, 2)]
        [TestCase(18, 0)]
        [TestCase(46, 1)]
        public void AGeneratorThatBreaksAnyCheckRefusesTheWholeCountedTable(int offset, byte invalid)
        {
            var record = Generator();
            record[offset] = invalid;
            Assert.That(this.locator.Locate(EmptyOwners(Generator(), record), GameVersion.HornOfTheAbyss), Is.Null);
        }

        [Test]
        public void AGuardedGeneratorIsAcceptedWithMatchingCreatureAndCount()
        {
            var record = Generator();
            BitConverter.GetBytes(54u).CopyTo(record, 18);
            BitConverter.GetBytes(3u).CopyTo(record, 46);
            Assert.That(this.locator.Locate(EmptyOwners(record), GameVersion.HornOfTheAbyss), Is.EqualTo(new MapObjectTables(4, 0, 6, 1)));
        }

        [Test]
        public void TheFinalGeneratorNeedsOnlyItsKnownPayloadNotTheFollowingBlocksFirstByte()
        {
            var bytes = EmptyOwners(Generator())[..^1];
            Assert.That(this.locator.Locate(bytes, GameVersion.HornOfTheAbyss), Is.EqualTo(new MapObjectTables(4, 0, 6, 1)));
        }
    }
}
