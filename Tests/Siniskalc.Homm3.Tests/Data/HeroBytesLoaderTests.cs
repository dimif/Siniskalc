using System.Text;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// How the heroes are found in the bytes of a savegame, on savegames made by hand: heroes are in no table, so each is found by the
    /// shape of its struct (skills used, a printable name, skill levels, spell flags, equipment and backpack slots, and for some games a
    /// tail), starting after the first 30000 bytes where campaign carry-over heroes may be.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class HeroBytesLoaderTests
    {
        private const int CarryOverLength = 30000;
        private const int SkillsUsed = 69;
        private const int Name = 195;
        private const int SkillSlots = 208;
        private const int Spells = 268;
        private const int Equipment = 408;
        private const int Faction = 26;

        private readonly HeroBytesLoader loader = new(Encoding.Latin1);

        public static IEnumerable<TestCaseData> Versions =>
        [
            new TestCaseData(GameVersion.RestorationOfErathia, 1064),
            new TestCaseData(GameVersion.ArmageddonsBlade, 1072),
            new TestCaseData(GameVersion.HeroesChronicles, 1082),
            new TestCaseData(GameVersion.ShadowOfDeath, 1094),
            new TestCaseData(GameVersion.HornOfTheAbyss, 1148),
        ];

        private static int EquipmentSlots(GameVersion version) => version is GameVersion.RestorationOfErathia or GameVersion.ArmageddonsBlade ? 18 : 19;

        private static int BackpackAt(GameVersion version) => Equipment + (EquipmentSlots(version) * 8) + (version == GameVersion.ArmageddonsBlade ? 8 : 0);

        private static int TailAt(GameVersion version) => BackpackAt(version) + 512;

        /// <summary>The bytes of a hero struct of the game: blank equipment slots, a name, and zeros for the rest.</summary>
        private static byte[] Struct(GameVersion version, int length, string name = "Orrin")
        {
            var bytes = new byte[length];
            for (var slot = 0; slot < EquipmentSlots(version); slot++)
            {
                Array.Fill(bytes, (byte)0xFF, Equipment + (slot * 8), 4);
            }

            Encoding.Latin1.GetBytes(name).CopyTo(bytes, Name);
            return bytes;
        }

        /// <summary>A savegame of zeros of <paramref name="total"/> bytes with each struct placed at its offset.</summary>
        private static byte[] Savegame(int total, params (int At, byte[] Hero)[] heroes)
        {
            var bytes = new byte[total];
            foreach (var (at, hero) in heroes)
            {
                hero.CopyTo(bytes, at);
            }

            return bytes;
        }

        private static byte[] OneHero(GameVersion version, int length, Action<byte[]>? change = null, int at = 31000, int trail = 100)
        {
            var hero = Struct(version, length);
            change?.Invoke(hero);
            return Savegame(at + length + trail, (at, hero));
        }

        [Test]
        public void AVersionWhoseHeroIsNotKnownHasNoHeroes()
        {
            Assert.That(this.loader.LoadHeroChunks(OneHero(GameVersion.ShadowOfDeath, 1094), GameVersion.Unknown), Is.Empty);
        }

        [Test]
        public void ASavegameOfThirtyThousandBytesOrLessHasNoHeroes()
        {
            var hero = Struct(GameVersion.ShadowOfDeath, 1094);

            Assert.That(this.loader.LoadHeroChunks(Savegame(CarryOverLength, (28000, hero)), GameVersion.ShadowOfDeath), Is.Empty);
            Assert.That(this.loader.LoadHeroChunks([], GameVersion.ShadowOfDeath), Is.Empty);
        }

        [TestCaseSource(nameof(Versions))]
        public void AHeroIsAChunkWithItsNameItsPlaceAndTheLengthOfTheStructOfTheGame(GameVersion version, int length)
        {
            var bytes = OneHero(version, length);

            var chunk = this.loader.LoadHeroChunks(bytes, version).Single();

            Assert.That((chunk.Name, chunk.StartIndex, chunk.Length), Is.EqualTo(("Orrin", 31000, length)));
            Assert.That(chunk.ToArray(), Is.EqualTo(bytes.AsSpan(31000, length).ToArray()));
            Assert.That(chunk.Biography, Is.Null);
        }

        [TestCaseSource(nameof(Versions))]
        public void AStructCutShortByTheEndOfTheSavegameIsNoHero(GameVersion version, int length)
        {
            var hero = Struct(version, length);
            var cut = version == GameVersion.HornOfTheAbyss ? 2 : 1; // one byte less is the 29 skill bytes of the older Horn of the Abyss saves
            var bytes = Savegame(31000 + length - cut, (31000, hero[..^cut]));

            Assert.That(this.loader.LoadHeroChunks(bytes, version), Is.Empty);
        }

        [Test]
        public void AHeroBeforeTheFirstThirtyThousandBytesIsACarryOverHeroAndIsSkipped()
        {
            var carryOver = Struct(GameVersion.ShadowOfDeath, 1094, "Carry");
            var real = Struct(GameVersion.ShadowOfDeath, 1094, "Real");
            var bytes = Savegame(34000, (28000, carryOver), (31000, real));

            var chunks = this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath);

            Assert.That(chunks.Select(c => c.Name), Is.EqualTo(new[] { "Real" }));
        }

        [Test]
        public void AHeroThatStartsExactlyAtThirtyThousandIsFound()
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, at: CarryOverLength);

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath).Single().StartIndex, Is.EqualTo(CarryOverLength));
        }

        [Test]
        public void SeveralHeroesAreFoundInTheOrderOfTheirBytes()
        {
            var bytes = Savegame(36000, (31000, Struct(GameVersion.ShadowOfDeath, 1094, "First")), (32500, Struct(GameVersion.ShadowOfDeath, 1094, "Second")), (34000, Struct(GameVersion.ShadowOfDeath, 1094, "Third")));

            var chunks = this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath);

            Assert.That(chunks.Select(c => (c.Name, c.StartIndex)), Is.EqualTo(new[] { ("First", 31000), ("Second", 32500), ("Third", 34000) }));
        }

        [Test]
        public void AHeroAfterTheFirstIsLookedForWithinFiveThousandBytesOfTheEndOfTheLastIncludingItsOwnLength()
        {
            var firstEnd = 31000 + 1094;
            var near = firstEnd + 5000 - 1094;
            var far = near + 1;

            var found = this.loader.LoadHeroChunks(Savegame(near + 1094, (31000, Struct(GameVersion.ShadowOfDeath, 1094, "First")), (near, Struct(GameVersion.ShadowOfDeath, 1094, "Second"))), GameVersion.ShadowOfDeath);
            var lost = this.loader.LoadHeroChunks(Savegame(far + 1094, (31000, Struct(GameVersion.ShadowOfDeath, 1094, "First")), (far, Struct(GameVersion.ShadowOfDeath, 1094, "Second"))), GameVersion.ShadowOfDeath);

            Assert.That(found.Select(c => c.Name), Is.EqualTo(new[] { "First", "Second" }));
            Assert.That(lost.Select(c => c.Name), Is.EqualTo(new[] { "First" }));
        }

        [Test]
        public void AHeroThatStartsOneByteBeforeThirtyThousandIsStillACarryOverHero()
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, at: CarryOverLength - 1);

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath), Is.Empty);
        }

        [Test]
        public void ACharacterOfTheNameBelowHexOneAEndsItAndOneFromHexOneAOnIsPartOfIt()
        {
            var inside = OneHero(GameVersion.ShadowOfDeath, 1094, h => Encoding.Latin1.GetBytes("Orrin").CopyTo(h, Name));
            var ending = OneHero(GameVersion.ShadowOfDeath, 1094, h => Encoding.Latin1.GetBytes("Orrin").CopyTo(h, Name));

            Assert.That(this.loader.LoadHeroChunks(inside, GameVersion.ShadowOfDeath), Has.Count.EqualTo(1));
            Assert.That(this.loader.LoadHeroChunks(ending, GameVersion.ShadowOfDeath), Is.Empty);
        }

        [Test]
        public void TheFirstHeroMayBeAnywhereAfterTheFirstThirtyThousandBytes()
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, at: 90000);

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath).Single().StartIndex, Is.EqualTo(90000));
        }

        [Test]
        public void NoHeroInTheBytesGivesNoChunks()
        {
            Assert.That(this.loader.LoadHeroChunks(new byte[40000], GameVersion.ShadowOfDeath), Is.Empty);
        }

        [TestCase(0x1C, true)]
        [TestCase(0x1D, false)]
        public void TheNumberOfSkillsUsedIsAtMostTwentyEight(int skills, bool found)
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, h => h[SkillsUsed] = (byte)skills);

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath), found ? Has.Count.EqualTo(1) : Is.Empty);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void TheNumberOfSkillsUsedIsAThirtyTwoBitNumberSoItsHighBytesAreZero(int offset)
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, h => h[SkillsUsed + offset] = 1);

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath), Is.Empty);
        }

        [TestCase(0x20)]
        [TestCase(0x00)]
        public void TheFirstCharacterOfTheNameIsPrintableAboveSpace(int first)
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, h => h[Name] = (byte)first);

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath), Is.Empty);
        }

        [Test]
        public void TheNameFillsAtMostTwelveBytesAndTheThirteenthIsZero()
        {
            var full = OneHero(GameVersion.ShadowOfDeath, 1094, h => Encoding.Latin1.GetBytes("Twelve chars").CopyTo(h, Name));
            var tooLong = OneHero(GameVersion.ShadowOfDeath, 1094, h =>
            {
                Encoding.Latin1.GetBytes("Twelve chars").CopyTo(h, Name);
                h[Name + 12] = (byte)'!';
            });

            Assert.That(this.loader.LoadHeroChunks(full, GameVersion.ShadowOfDeath).Single().Name, Is.EqualTo("Twelve chars"));
            Assert.That(this.loader.LoadHeroChunks(tooLong, GameVersion.ShadowOfDeath), Is.Empty);
        }

        [Test]
        public void ANameEndsAtTheFirstControlCharacterAndOnlyZerosMayFollow()
        {
            var padded = OneHero(GameVersion.ShadowOfDeath, 1094);
            var dirty = OneHero(GameVersion.ShadowOfDeath, 1094, h => h[Name + 8] = (byte)'x');

            Assert.That(this.loader.LoadHeroChunks(padded, GameVersion.ShadowOfDeath).Single().Name, Is.EqualTo("Orrin"));
            Assert.That(this.loader.LoadHeroChunks(dirty, GameVersion.ShadowOfDeath), Is.Empty);
        }

        [Test]
        public void ANameOfNothingButFFBytesIsNoHero()
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, h =>
            {
                Array.Fill(h, (byte)0xFF, Name, 12);
                h[Name + 12] = 0;
            });

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath), Is.Empty);
        }

        [Test]
        public void ANameThatIsNoTextInTheEncodingOfTheSavegameIsNoHeroAndTheScanGoesOn()
        {
            var strict = new HeroBytesLoader(Encoding.GetEncoding("us-ascii", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback));
            var bytes = Savegame(
                35000,
                (31000, Struct(GameVersion.ShadowOfDeath, 1094, "Café")),
                (32500, Struct(GameVersion.ShadowOfDeath, 1094, "Orrin")));

            var chunks = strict.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath);

            Assert.That(chunks.Select(c => c.Name), Is.EqualTo(new[] { "Orrin" }));
        }

        [Test]
        public void TheNameIsReadInTheEncodingGiven()
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, h => h[Name + 3] = 0xE9);

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath).Single().Name, Is.EqualTo("Orrén"));
        }

        [TestCase(3, true)]
        [TestCase(4, false)]
        public void TheFirstTwentyEightSkillBytesAreLevelsFromZeroToThree(int level, bool found)
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, h => h[SkillSlots + 27] = (byte)level);

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath), found ? Has.Count.EqualTo(1) : Is.Empty);
        }

        [TestCase(0x1C, true)]
        [TestCase(0x1D, false)]
        public void TheTwentyEightSkillSlotBytesAfterThemAreAtMostTwentyEightInOlderGames(int slot, bool found)
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, h => h[SkillSlots + 28 + 27] = (byte)slot);

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath), found ? Has.Count.EqualTo(1) : Is.Empty);
        }

        [Test]
        public void HornOfTheAbyssHasThirtySkillBytesOfLevelsAndNoSecondBlockRightAfter()
        {
            var accepted = OneHero(GameVersion.HornOfTheAbyss, 1148, h => h[SkillSlots + 29] = 3);
            var refused = OneHero(GameVersion.HornOfTheAbyss, 1148, h => h[SkillSlots + 29] = 4);
            var anything = OneHero(GameVersion.HornOfTheAbyss, 1148, h => h[SkillSlots + 30] = 0x1C);

            Assert.That(this.loader.LoadHeroChunks(accepted, GameVersion.HornOfTheAbyss), Has.Count.EqualTo(1));
            Assert.That(this.loader.LoadHeroChunks(refused, GameVersion.HornOfTheAbyss), Is.Empty);
            Assert.That(this.loader.LoadHeroChunks(anything, GameVersion.HornOfTheAbyss), Has.Count.EqualTo(1));
        }

        [TestCase(1, true)]
        [TestCase(2, false)]
        public void EverySpellByteIsZeroOrOne(int value, bool found)
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, h => h[Spells + 139] = (byte)value);

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath), found ? Has.Count.EqualTo(1) : Is.Empty);
        }

        [Test]
        public void AnEquipmentSlotIsBlankOrAByteFollowedByThreeZeros()
        {
            var artifact = OneHero(GameVersion.ShadowOfDeath, 1094, h => Array.Copy(new byte[] { 5, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF }, 0, h, Equipment, 8));
            var broken = OneHero(GameVersion.ShadowOfDeath, 1094, h => Array.Copy(new byte[] { 1, 1, 0, 0 }, 0, h, Equipment + 8, 4));

            Assert.That(this.loader.LoadHeroChunks(artifact, GameVersion.ShadowOfDeath), Has.Count.EqualTo(1));
            Assert.That(this.loader.LoadHeroChunks(broken, GameVersion.ShadowOfDeath), Is.Empty);
        }

        [Test]
        public void AnEquipmentSectionOfNothingButZerosIsNoHero()
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, h => Array.Clear(h, Equipment, 19 * 8));

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath), Is.Empty);
        }

        [Test]
        public void AnArtifactSlotScrollSlotAndWarMachineSlotAreEquipmentAndOtherLastHalvesAreNot()
        {
            var last = Equipment + (18 * 8);
            var artifact = OneHero(GameVersion.ShadowOfDeath, 1094, h =>
            {
                Array.Clear(h, last, 4);
                h[last] = 7;
                Array.Fill(h, (byte)0xFF, last + 4, 4);
            });
            var machine = OneHero(GameVersion.ShadowOfDeath, 1094, h =>
            {
                Array.Clear(h, last, 8);
                h[last] = 7;
                h[last + 4] = 1;
                h[last + 5] = 1;
            });
            var other = OneHero(GameVersion.ShadowOfDeath, 1094, h =>
            {
                Array.Clear(h, last, 8);
                h[last] = 7;
                h[last + 4] = 1;
                h[last + 6] = 1;
            });

            Assert.That(this.loader.LoadHeroChunks(artifact, GameVersion.ShadowOfDeath), Has.Count.EqualTo(1));
            Assert.That(this.loader.LoadHeroChunks(machine, GameVersion.ShadowOfDeath), Has.Count.EqualTo(1));
            Assert.That(this.loader.LoadHeroChunks(other, GameVersion.ShadowOfDeath), Is.Empty);
        }

        [Test]
        public void AnEarlierSlotThatIsNotEquipmentIsCheckedOnlyUpToTheLastSlotThatStartsWithANonZeroByte()
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, h =>
            {
                Array.Clear(h, Equipment, 19 * 8);
                Array.Fill(h, (byte)0xFF, Equipment + (18 * 8), 4);
                Array.Copy(new byte[] { 0, 3, 0, 0, 0, 0, 0, 0 }, 0, h, Equipment + 8, 8);
            });

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath), Has.Count.EqualTo(1));
        }

        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(2, true)]
        public void ABackpackSlotNumberIsBlankOrAByteFollowedByThreeZeros(int slotByte, bool found)
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, h =>
            {
                var at = BackpackAt(GameVersion.ShadowOfDeath) + 60;
                if (slotByte == 0)
                {
                    h[at] = 5;
                    h[at + 1] = 1;
                }
                else if (slotByte == 1)
                {
                    Array.Fill(h, (byte)0xFF, at, 3);
                }
                else
                {
                    Array.Fill(h, (byte)0xFF, at, 4);
                    h[at + 4] = 9;
                }
            });

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath), found ? Has.Count.EqualTo(1) : Is.Empty);
        }

        [Test]
        public void TheLastBackpackSlotIsPartOfTheStruct()
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, h => h[BackpackAt(GameVersion.ShadowOfDeath) + 511] = 1);

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath), Is.Empty);
        }

        [TestCase(GameVersion.ShadowOfDeath, 1094)]
        [TestCase(GameVersion.HornOfTheAbyss, 1148)]
        [TestCase(GameVersion.HeroesChronicles, 1082)]
        public void TheTailAfterTheBackpackHasSixFlagsAndThreeSmallNumbers(GameVersion version, int length)
        {
            var tail = TailAt(version);

            foreach (var (offset, value, found) in new[] { (1, 1, true), (1, 2, false), (6, 2, false), (7, 2, true), (7, 3, false), (8, 1, true), (8, 2, false), (9, 5, true), (9, 6, false) })
            {
                var bytes = OneHero(version, length, h => h[tail + offset] = (byte)value);

                Assert.That(this.loader.LoadHeroChunks(bytes, version), found ? Has.Count.EqualTo(1) : Is.Empty, $"byte {offset} = {value}");
            }
        }

        [TestCase(GameVersion.RestorationOfErathia, 1064)]
        [TestCase(GameVersion.ArmageddonsBlade, 1072)]
        public void TheOlderGamesHaveNoTailSoWhateverFollowsTheBackpackIsLeftAlone(GameVersion version, int length)
        {
            var tail = TailAt(version);
            var bytes = OneHero(version, length, trail: 100);
            bytes[31000 + tail + 7] = 9;

            Assert.That(this.loader.LoadHeroChunks(bytes, version), Has.Count.EqualTo(1));
        }

        [Test]
        public void ShadowOfDeathHoldsTwelveMoreBytesAfterTheTailThatBelongToTheHero()
        {
            var plain = Struct(GameVersion.ShadowOfDeath, 1094);
            plain[1093] = 1;
            var bytes = Savegame(31000 + 1094 + 50, (31000, plain));

            var chunk = this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath).Single();

            Assert.That(chunk.Length, Is.EqualTo(1094));
            Assert.That(chunk.ToArray()[^1], Is.EqualTo(1));
        }

        [Test]
        public void HornOfTheAbyssAcceptsTwentyNineSkillSlotBytesAfterTheGapInTheOlderSaves()
        {
            var hero = Struct(GameVersion.HornOfTheAbyss, 1148);
            hero[1118 + 29] = 0x40;
            var old = Savegame(31000 + 1148 + 100, (31000, hero));

            Assert.That(this.loader.LoadHeroChunks(old, GameVersion.HornOfTheAbyss).Single().Length, Is.EqualTo(1147));
        }

        [Test]
        public void HornOfTheAbyssWithNoValidSkillSlotsAfterTheGapIsNoHero()
        {
            var bytes = OneHero(GameVersion.HornOfTheAbyss, 1148, h => h[1118 + 5] = 0x1D);

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.HornOfTheAbyss), Is.Empty);
        }

        [Test]
        public void AHeroWithNoBiographyHasNoHeader()
        {
            var chunk = this.loader.LoadHeroChunks(OneHero(GameVersion.ShadowOfDeath, 1094), GameVersion.ShadowOfDeath).Single();

            Assert.That(chunk.Biography, Is.Null);
            Assert.That(chunk.OutsidePositionBytes, Is.Null);
        }

        [Test]
        public void ABiographyIsTheTextRightBeforeTheFactionByteWhoseLengthIsTheFourBytesBeforeIt()
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, at: 31000);
            Encoding.Latin1.GetBytes("Hello").CopyTo(bytes, 31000 + Faction - 5);
            BitConverter.GetBytes(5).CopyTo(bytes, 31000 + Faction - 5 - 4);

            var chunk = this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath).Single();

            Assert.That(chunk.Biography, Is.EqualTo("Hello"));
            Assert.That(chunk.HeaderIndex, Is.EqualTo(31000 - 5));
            Assert.That(chunk.StartIndex, Is.EqualTo(31000));
        }

        [Test]
        public void ATextWhoseLengthDoesNotAgreeWithTheFourBytesBeforeItIsNoBiography()
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, at: 31000);
            Encoding.Latin1.GetBytes("Hello").CopyTo(bytes, 31000 + Faction - 5);
            BitConverter.GetBytes(4).CopyTo(bytes, 31000 + Faction - 5 - 4);

            Assert.That(this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath).Single().Biography, Is.Null);
        }

        [Test]
        public void ABiographyThatIsNoTextInTheEncodingIsLeftOutAndTheHeroIsKept()
        {
            var strict = new HeroBytesLoader(Encoding.GetEncoding("us-ascii", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback));
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, at: 31000);
            Encoding.Latin1.GetBytes("Café!").CopyTo(bytes, 31000 + Faction - 5);
            BitConverter.GetBytes(5).CopyTo(bytes, 31000 + Faction - 5 - 4);

            var chunk = strict.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath).Single();

            Assert.That(chunk.Name, Is.EqualTo("Orrin"));
            Assert.That(chunk.Biography, Is.Null);
        }

        [Test]
        public void TheHeaderOfAHeroWithABiographyIsTheTwentySixBytesInFrontOfTheTextAndTheFactionByte()
        {
            var bytes = OneHero(GameVersion.ShadowOfDeath, 1094, at: 31000);
            Encoding.Latin1.GetBytes("Hello").CopyTo(bytes, 31000 + Faction - 5);
            BitConverter.GetBytes(5).CopyTo(bytes, 31000 + Faction - 5 - 4);
            bytes[31000 - 5] = 0xAA;

            var chunk = this.loader.LoadHeroChunks(bytes, GameVersion.ShadowOfDeath).Single();

            Assert.That(chunk.OutsidePositionBytes, Is.EqualTo(bytes.AsSpan(31000 - 5, 6).ToArray()));
            Assert.That(chunk.GetHeaderByteAt(0), Is.EqualTo(0xAA));
        }
    }
}
