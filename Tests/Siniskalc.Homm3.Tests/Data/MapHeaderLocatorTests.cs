using System.Text;
using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// How the name, description, difficulty, size and underground flag of the map are found at the start of a savegame, on headers
    /// made by hand: a byte, the width as 32 bits, the underground byte, then the name and the description each with a 16-bit length,
    /// then the difficulty byte.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class MapHeaderLocatorTests
    {
        private readonly MapHeaderLocator locator = new();

        /// <summary>A savegame of zeros, then 0xFF after the header so no stray text can be read as a second header, with the header of a map starting <paramref name="before"/> bytes in (the name length is at <paramref name="before"/> + 6).</summary>
        private static byte[] Savegame(string name, string description, int difficulty, int size = 36, int underground = 0, int before = 20, int padding = 100)
        {
            var bytes = new List<byte>(new byte[before]);
            bytes.Add(1);
            bytes.AddRange(BitConverter.GetBytes(size));
            bytes.Add((byte)underground);
            bytes.AddRange(BitConverter.GetBytes((ushort)name.Length));
            bytes.AddRange(Encoding.Latin1.GetBytes(name));
            bytes.AddRange(BitConverter.GetBytes((ushort)description.Length));
            bytes.AddRange(Encoding.Latin1.GetBytes(description));
            bytes.Add((byte)difficulty);
            bytes.AddRange(Enumerable.Repeat((byte)0xFF, padding));
            return [.. bytes];
        }

        [Test]
        public void TheNameTheDescriptionAndTheDifficultyOfTheMapAreRead()
        {
            var header = this.locator.Find(Savegame("Bios", "A small map.", 3), GameVersion.ShadowOfDeath);

            Assert.That(header, Is.Not.Null);
            Assert.That((header!.Name, header.Description, header.Difficulty), Is.EqualTo(("Bios", "A small map.", GameDifficulty.Queen)));
        }

        [TestCase(0, GameDifficulty.Pawn)]
        [TestCase(1, GameDifficulty.Knight)]
        [TestCase(2, GameDifficulty.Rook)]
        [TestCase(3, GameDifficulty.Queen)]
        [TestCase(4, GameDifficulty.King)]
        public void TheDifficultyByteGoesFromPawnToKing(int code, GameDifficulty expected)
        {
            Assert.That(this.locator.Find(Savegame("Bios", "x", code), GameVersion.ShadowOfDeath)!.Difficulty, Is.EqualTo(expected));
        }

        [Test]
        public void ADifficultyByteAboveKingIsNoHeader()
        {
            Assert.That(this.locator.Find(Savegame("Bios", "x", 5), GameVersion.ShadowOfDeath), Is.Null);
        }

        [Test]
        public void TheSizeIsTheWidthFiveBytesBeforeTheNameAndTheUndergroundIsTheByteBeforeIt()
        {
            var flat = this.locator.Find(Savegame("Bios", "x", 0, size: 72, underground: 0), GameVersion.ShadowOfDeath)!;
            var deep = this.locator.Find(Savegame("Bios", "x", 0, size: 144, underground: 1), GameVersion.ShadowOfDeath)!;

            Assert.That((flat.Size, flat.HasUnderground), Is.EqualTo((72, false)));
            Assert.That((deep.Size, deep.HasUnderground), Is.EqualTo((144, true)));
        }

        [TestCase(0)]
        [TestCase(253)]
        [TestCase(-5)]
        public void AWidthOutsideWhatAMapCanBeIsReadAsNoSize(int size)
        {
            Assert.That(this.locator.Find(Savegame("Bios", "x", 0, size: size), GameVersion.ShadowOfDeath)!.Size, Is.EqualTo(0));
        }

        [Test]
        public void TheLargestWidthAMapCanHaveIsKept()
        {
            Assert.That(this.locator.Find(Savegame("Bios", "x", 0, size: 252), GameVersion.ShadowOfDeath)!.Size, Is.EqualTo(252));
        }

        [Test]
        public void AnUndergroundByteOtherThanOneMeansNoUnderground()
        {
            Assert.That(this.locator.Find(Savegame("Bios", "x", 0, underground: 2), GameVersion.ShadowOfDeath)!.HasUnderground, Is.False);
        }

        [Test]
        public void ANameAtTheVeryStartHasNoRoomInFrontForTheSizeSoItIsNoSizeAndNoUnderground()
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes((ushort)4));
            bytes.AddRange(Encoding.ASCII.GetBytes("Bios"));
            bytes.AddRange(BitConverter.GetBytes((ushort)1));
            bytes.AddRange(Encoding.ASCII.GetBytes("x"));
            bytes.Add(2);
            bytes.AddRange(new byte[50]);

            var header = this.locator.Find([.. bytes], GameVersion.ShadowOfDeath)!;

            Assert.That((header.Name, header.Size, header.HasUnderground), Is.EqualTo(("Bios", 0, false)));
        }

        [Test]
        public void LocateGivesWhereTheNameAndTheDescriptionAre()
        {
            var location = this.locator.Locate(Savegame("Bios", "A small map.", 3, before: 20), GameVersion.ShadowOfDeath)!;

            Assert.That((location.NameLengthAt, location.NameLength, location.DescriptionLengthAt, location.DescriptionLength), Is.EqualTo((26, 4, 32, 12)));
            Assert.That(location.DifficultyAt, Is.EqualTo(46));
        }

        [TestCase(GameVersion.Unknown)]
        [TestCase(GameVersion.HeroesChronicles)]
        public void AVersionWithNoGameSetupHasNoHeader(GameVersion version)
        {
            var bytes = Savegame("Bios", "x", 0);

            Assert.That(this.locator.Locate(bytes, version), Is.Null);
            Assert.That(this.locator.Find(bytes, version), Is.Null);
        }

        [TestCase(GameVersion.RestorationOfErathia)]
        [TestCase(GameVersion.ArmageddonsBlade)]
        [TestCase(GameVersion.ShadowOfDeath)]
        [TestCase(GameVersion.HornOfTheAbyss)]
        public void EveryVersionWithAGameSetupFindsTheHeader(GameVersion version)
        {
            Assert.That(this.locator.Find(Savegame("Bios", "x", 0), version)!.Name, Is.EqualTo("Bios"));
        }

        [Test]
        public void BytesWithNoTextHaveNoHeader()
        {
            Assert.That(this.locator.Find(new byte[300], GameVersion.ShadowOfDeath), Is.Null);
            Assert.That(this.locator.Find([], GameVersion.ShadowOfDeath), Is.Null);
        }

        [Test]
        public void ANameOfSixtyCharactersIsAHeaderAndOfSixtyOneIsNot()
        {
            Assert.That(this.locator.Find(Savegame(new string('a', 60), "x", 0), GameVersion.ShadowOfDeath), Is.Not.Null);
            Assert.That(this.locator.Find(Savegame(new string('a', 61), "x", 0), GameVersion.ShadowOfDeath), Is.Null);
        }

        [Test]
        public void AnEmptyNameIsNoHeader()
        {
            Assert.That(this.locator.Find(Savegame(string.Empty, "x", 0), GameVersion.ShadowOfDeath), Is.Null);
        }

        [Test]
        public void ANameWithAByteThatIsNoTextIsNoHeader()
        {
            var bytes = Savegame("Bios", "x", 0);
            bytes[28] = 0x01;

            Assert.That(this.locator.Find(bytes, GameVersion.ShadowOfDeath), Is.Null);
        }

        [Test]
        public void ADescriptionMayBeEmptyAndMayHoldLineBreaks()
        {
            Assert.That(this.locator.Find(Savegame("Bios", string.Empty, 0), GameVersion.ShadowOfDeath)!.Description, Is.Empty);
            Assert.That(this.locator.Find(Savegame("Bios", "One\r\nTwo", 0), GameVersion.ShadowOfDeath)!.Description, Is.EqualTo("One\r\nTwo"));
        }

        [Test]
        public void ADescriptionWithAByteThatIsNoTextIsNoHeader()
        {
            var bytes = Savegame("Bios", "abc", 0);
            bytes[34] = 0x07;

            Assert.That(this.locator.Find(bytes, GameVersion.ShadowOfDeath), Is.Null);
        }

        [Test]
        public void ADescriptionThatRunsPastTheEndOfTheBytesIsNoHeader()
        {
            var bytes = Savegame("Bios", "A small map.", 0, padding: 0);

            Assert.That(this.locator.Find(bytes[..^1], GameVersion.ShadowOfDeath), Is.Null);
        }

        [Test]
        public void TheHeaderIsLookedForInTheFirstTwoHundredBytesOnly()
        {
            Assert.That(this.locator.Find(Savegame("Bios", "x", 0, before: 193), GameVersion.ShadowOfDeath), Is.Not.Null);
            Assert.That(this.locator.Find(Savegame("Bios", "x", 0, before: 194), GameVersion.ShadowOfDeath), Is.Null);
        }

        [Test]
        public void ASavegameOfHornOfTheAbyssAndOneOfShadowOfDeathAreFoundAtTheirOwnPlaces()
        {
            var sod = this.locator.Locate(Savegame("Bios", "x", 0, before: 52), GameVersion.ShadowOfDeath)!;
            var hota = this.locator.Locate(Savegame("Bios", "x", 0, before: 89), GameVersion.HornOfTheAbyss)!;

            Assert.That(sod.NameLengthAt, Is.EqualTo(58));
            Assert.That(hota.NameLengthAt, Is.EqualTo(95));
        }
    }
}
