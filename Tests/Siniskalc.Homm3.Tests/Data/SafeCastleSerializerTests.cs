using Siniskalc.Core.Models;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests, on a castle record of zeros, which bytes the writer of a castle changes for each field of the model: the heroes, the
    /// Mage Guild (level, counts, spells), the creatures that wait in the dwellings and the three masks of buildings. The savefile
    /// based checks of the same writer are in <c>SafeCastleSerializerSavegameTests</c>.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeCastleSerializerTests
    {
        private const int NameLength = 4;
        private const int NameEnd = CastleLayout.Name + NameLength;

        private readonly SafeCastleSerializer serializer = new();

        /// <summary>A record of zeros with a name of four bytes, whose slots of Mage Guild spells hold -1 (no spell).</summary>
        private static CastleChunk Blank()
        {
            var bytes = new byte[NameEnd + CastleLayout.ChunkEndFromNameEnd];
            BitConverter.GetBytes((ushort)NameLength).CopyTo(bytes, CastleLayout.NameLength);
            Array.Fill(bytes, (byte)0xFF, NameEnd + CastleLayout.SpellListsFromNameEnd, CastleLayout.SpellListsLength);
            return new CastleChunk("Test", bytes, 0);
        }

        private static CastleModel Rampart() => CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Rampart"));

        private static ulong Mask(CastleChunk chunk, int fromNameEnd) => BitConverter.ToUInt64(chunk.ToArray(), NameEnd + fromNameEnd);

        private static ulong Bits(IEnumerable<Building> buildings) => buildings.Aggregate(0UL, (mask, b) => mask | (1UL << b.Code));

        private CastleChunk Write(CastleChunk chunk, CastleModel castle) => this.serializer.Serialize(chunk, castle);

        private int[] ChangedOffsets(CastleModel before, CastleModel after)
        {
            var blank = Blank();
            return ByteDiff.Of(this.Write(blank, before).ToArray(), this.Write(blank, after).ToArray()).Select(d => d.Offset).ToArray();
        }

        [Test]
        public void TheHeroesAreWrittenAsOneByteEachAndNoHeroIsFF()
        {
            var written = this.Write(Blank(), Rampart() with { GarrisonedHeroId = 7, VisitingHeroId = 254 });
            var none = this.Write(Blank(), Rampart() with { GarrisonedHeroId = null, VisitingHeroId = null });

            Assert.That((written.GetByteAt(CastleLayout.GarrisonedHero), written.GetByteAt(CastleLayout.VisitingHero)), Is.EqualTo(((byte)7, (byte)254)));
            Assert.That((none.GetByteAt(CastleLayout.GarrisonedHero), none.GetByteAt(CastleLayout.VisitingHero)), Is.EqualTo(((byte)0xFF, (byte)0xFF)));
        }

        [Test]
        public void ChangingTheHeroesChangesOnlyTheirTwoBytes()
        {
            var changed = this.ChangedOffsets(Rampart(), Rampart() with { GarrisonedHeroId = 1, VisitingHeroId = 2 });

            Assert.That(changed, Is.EquivalentTo(new[] { CastleLayout.GarrisonedHero, CastleLayout.VisitingHero }));
        }

        [TestCase(-1)]
        [TestCase(255)]
        public void AHeroThatDoesNotFitAByteBelowFFIsRefused(int heroId)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => this.Write(Blank(), Rampart() with { GarrisonedHeroId = heroId }));
            Assert.Throws<ArgumentOutOfRangeException>(() => this.Write(Blank(), Rampart() with { VisitingHeroId = heroId }));
        }

        [Test]
        public void TheMageGuildLevelIsTheHighestGuildBuilt()
        {
            var none = this.Write(Blank(), Rampart());
            var three = this.Write(Blank(), CastleModels.With(Rampart(), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild1"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild2"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild3")));

            Assert.That((none.GetByteAt(CastleLayout.MageGuildLevel), three.GetByteAt(CastleLayout.MageGuildLevel)), Is.EqualTo(((byte)0, (byte)3)));
        }

        [Test]
        public void TheSpellCountsOfTheFiveLevelsAreWrittenInOrder()
        {
            var written = this.Write(Blank(), Rampart() with { MageGuildSpellCounts = Numbers.Of(1, 2, 3, 4, 5) });

            Assert.That(written.GetBytesAt(NameEnd + CastleLayout.SpellCountsFromNameEnd, 5), Is.EqualTo(new byte[] { 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void AListOfFewerCountsThanLevelsLeavesTheOtherLevelsAlone()
        {
            var chunk = Blank();
            var bytes = chunk.ToArray();
            bytes[NameEnd + CastleLayout.SpellCountsFromNameEnd + 4] = 9;

            var written = this.Write(chunk.WithBytes(bytes), Rampart() with { MageGuildSpellCounts = Numbers.Of(7, 8) });

            Assert.That(written.GetBytesAt(NameEnd + CastleLayout.SpellCountsFromNameEnd, 5), Is.EqualTo(new byte[] { 7, 8, 0, 0, 9 }));
        }

        [Test]
        public void ASpellCountAboveAByteIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => this.Write(Blank(), Rampart() with { MageGuildSpellCounts = Numbers.Of(0, 0, 256, 0, 0) }));
            Assert.DoesNotThrow(() => this.Write(Blank(), Rampart() with { MageGuildSpellCounts = Numbers.Of(0, 0, 255, 0, 0) }));
        }

        [Test]
        public void TheSpellsOfAGuildAreWrittenAsLittleEndianCodesIntoTheSlotsOfTheirLevel()
        {
            var chunk = Blank();
            var bytes = chunk.ToArray();
            foreach (var (level, slot) in new[] { (1, 0), (1, 1), (2, 0) })
            {
                BitConverter.GetBytes((int)TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless").Code).CopyTo(bytes, MageGuildSpellSlots.Offset(chunk, level, slot));
            }

            var spells = Rampart().MageGuildSpells.ToList();
            spells[0] = [TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow")];
            spells[1] = [TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless")];

            var written = this.Write(chunk.WithBytes(bytes), Rampart() with { MageGuildSpells = spells });

            Assert.That(written.GetInt32At(MageGuildSpellSlots.Offset(written, 1, 0)), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste").Code));
            Assert.That(written.GetInt32At(MageGuildSpellSlots.Offset(written, 1, 1)), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow").Code));
            Assert.That(written.GetInt32At(MageGuildSpellSlots.Offset(written, 2, 0)), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless").Code));
            Assert.That(written.GetBytesAt(MageGuildSpellSlots.Offset(written, 1, 0), 4), Is.EqualTo(new byte[] { 53, 0, 0, 0 }));
        }

        [Test]
        public void ASlotThatHoldsNoKnownSpellIsNotWrittenAndTheSpellsFillOnlyTheKnownSlots()
        {
            var chunk = Blank();
            var bytes = chunk.ToArray();
            BitConverter.GetBytes((int)TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless").Code).CopyTo(bytes, MageGuildSpellSlots.Offset(chunk, 1, 1));
            BitConverter.GetBytes((int)TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless").Code).CopyTo(bytes, MageGuildSpellSlots.Offset(chunk, 1, 3));
            var spells = Rampart().MageGuildSpells.ToList();
            spells[0] = [TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste"), TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow")];

            var written = this.Write(chunk.WithBytes(bytes), Rampart() with { MageGuildSpells = spells });

            Assert.That(written.GetInt32At(MageGuildSpellSlots.Offset(written, 1, 0)), Is.EqualTo(-1));
            Assert.That(written.GetInt32At(MageGuildSpellSlots.Offset(written, 1, 1)), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste").Code));
            Assert.That(written.GetInt32At(MageGuildSpellSlots.Offset(written, 1, 2)), Is.EqualTo(-1));
            Assert.That(written.GetInt32At(MageGuildSpellSlots.Offset(written, 1, 3)), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow").Code));
        }

        [Test]
        public void ALevelWhoseListIsNotAsLongAsTheKnownSlotsIsLeftAsItIs()
        {
            var chunk = Blank();
            var bytes = chunk.ToArray();
            BitConverter.GetBytes((int)TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless").Code).CopyTo(bytes, MageGuildSpellSlots.Offset(chunk, 1, 0));
            BitConverter.GetBytes((int)TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless").Code).CopyTo(bytes, MageGuildSpellSlots.Offset(chunk, 1, 1));
            var odd = chunk.WithBytes(bytes);
            var spells = Rampart().MageGuildSpells.ToList();
            spells[0] = [TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste")];

            var written = this.Write(odd, Rampart() with { MageGuildSpells = spells });

            Assert.That(written.GetInt32At(MageGuildSpellSlots.Offset(written, 1, 0)), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless").Code));
            Assert.That(written.GetInt32At(MageGuildSpellSlots.Offset(written, 1, 1)), Is.EqualTo(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless").Code));
        }

        [Test]
        public void TheCreaturesWaitingAreWrittenAsLittleEndianShortsInTheSlotOfTheDwellingOfTheirLevel()
        {
            var written = this.Write(Blank(), Rampart() with { CreaturesAvailable = Numbers.Of(0, 0, 0, 1500, 0, 0, 65535) });

            Assert.That(written.GetBytesAt(NameEnd + CastleLayout.CreaturesAvailableFromNameEnd + 6, 2), Is.EqualTo(new byte[] { 0xDC, 0x05 }));
            Assert.That(written.GetBytesAt(NameEnd + CastleLayout.CreaturesAvailableFromNameEnd + 12, 2), Is.EqualTo(new byte[] { 0xFF, 0xFF }));
            Assert.That(written.GetBytesAt(NameEnd + CastleLayout.UpgradedCreaturesAvailableFromNameEnd + 6, 2), Is.EqualTo(new byte[] { 0, 0 }));
        }

        [Test]
        public void WithTheUpgradeBuiltTheCreaturesAreWrittenInTheSlotOfTheUpgrade()
        {
            var castle = CastleModels.With(Rampart(), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgCentaurStables"));

            var written = this.Write(Blank(), castle with { CreaturesAvailable = Numbers.Of(12, 0, 0, 0, 0, 0, 0) });

            Assert.That(written.GetUInt16At(NameEnd + CastleLayout.UpgradedCreaturesAvailableFromNameEnd), Is.EqualTo(12));
            Assert.That(written.GetUInt16At(NameEnd + CastleLayout.CreaturesAvailableFromNameEnd), Is.EqualTo(0));
        }

        [Test]
        public void ACountThatWasInTheSlotOfTheDwellingMovesToTheUpgradeWhenTheUpgradeIsBuilt()
        {
            var chunk = Blank();
            var bytes = chunk.ToArray();
            BitConverter.GetBytes((ushort)12).CopyTo(bytes, NameEnd + CastleLayout.CreaturesAvailableFromNameEnd);
            var castle = CastleModels.With(Rampart(), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgCentaurStables"));

            var written = this.Write(chunk.WithBytes(bytes), castle with { CreaturesAvailable = Numbers.Of(12, 0, 0, 0, 0, 0, 0) });

            Assert.That(written.GetUInt16At(NameEnd + CastleLayout.UpgradedCreaturesAvailableFromNameEnd), Is.EqualTo(12));
            Assert.That(written.GetUInt16At(NameEnd + CastleLayout.CreaturesAvailableFromNameEnd), Is.EqualTo(0));
        }

        [Test]
        public void ACountThatWasInTheSlotOfTheUpgradeMovesBackWhenTheUpgradeIsTakenDown()
        {
            var chunk = Blank();
            var bytes = chunk.ToArray();
            BitConverter.GetBytes((ushort)12).CopyTo(bytes, NameEnd + CastleLayout.UpgradedCreaturesAvailableFromNameEnd);

            var written = this.Write(chunk.WithBytes(bytes), CastleModels.With(Rampart(), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables")) with { CreaturesAvailable = Numbers.Of(12, 0, 0, 0, 0, 0, 0) });

            Assert.That(written.GetUInt16At(NameEnd + CastleLayout.CreaturesAvailableFromNameEnd), Is.EqualTo(12));
            Assert.That(written.GetUInt16At(NameEnd + CastleLayout.UpgradedCreaturesAvailableFromNameEnd), Is.EqualTo(0));
        }

        [Test]
        public void ASlotTheCountWasNotInIsLeftAsItIsWhenTheCountInTheActiveSlotChanges()
        {
            var chunk = Blank();
            var bytes = chunk.ToArray();
            BitConverter.GetBytes((ushort)5).CopyTo(bytes, NameEnd + CastleLayout.CreaturesAvailableFromNameEnd);
            BitConverter.GetBytes((ushort)9).CopyTo(bytes, NameEnd + CastleLayout.UpgradedCreaturesAvailableFromNameEnd);
            var castle = CastleModels.With(Rampart(), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartUpgCentaurStables"));

            var written = this.Write(chunk.WithBytes(bytes), castle with { CreaturesAvailable = Numbers.Of(20, 0, 0, 0, 0, 0, 0) });

            Assert.That(written.GetUInt16At(NameEnd + CastleLayout.UpgradedCreaturesAvailableFromNameEnd), Is.EqualTo(20));
            Assert.That(written.GetUInt16At(NameEnd + CastleLayout.CreaturesAvailableFromNameEnd), Is.EqualTo(5));
        }

        [Test]
        public void ACountAboveAShortOrBelowZeroIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => this.Write(Blank(), Rampart() with { CreaturesAvailable = Numbers.Of(65536, 0, 0, 0, 0, 0, 0) }));
            Assert.Throws<ArgumentOutOfRangeException>(() => this.Write(Blank(), Rampart() with { CreaturesAvailable = Numbers.Of(0, -1, 0, 0, 0, 0, 0) }));
        }

        [Test]
        public void EachBuildingSetsItsBitInTheMaskOfBuiltBuildingsOfThoseVisibleAndOfThoseEnabled()
        {
            var castle = Rampart() with
            {
                Buildings = new HashSet<Building> { TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("VillageHall"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables") },
                VisibleBuildings = new HashSet<Building> { TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("VillageHall") },
                EnabledBuildings = new HashSet<Building> { TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild1") },
            };

            var written = this.Write(Blank(), castle);

            Assert.That(Mask(written, CastleLayout.BuiltBuildingsFromNameEnd), Is.EqualTo(Bits([TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("VillageHall"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables")])));
            Assert.That(Mask(written, CastleLayout.VisibleBuildingsFromNameEnd), Is.EqualTo(Bits([TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("VillageHall")])));
            Assert.That(Mask(written, CastleLayout.EnabledBuildingsFromNameEnd), Is.EqualTo(Bits([TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("MagesGuild1")])));
        }

        [Test]
        public void ABitOfABuildingThatTheModelNoLongerHasIsCleared()
        {
            var chunk = Blank();
            var bytes = chunk.ToArray();
            BitConverter.GetBytes(Bits([TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("VillageHall"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables")])).CopyTo(bytes, NameEnd + CastleLayout.BuiltBuildingsFromNameEnd);
            BitConverter.GetBytes(Bits([TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("RampartCentaurStables")])).CopyTo(bytes, NameEnd + CastleLayout.VisibleBuildingsFromNameEnd);

            var written = this.Write(chunk.WithBytes(bytes), Rampart() with { VisibleBuildings = new HashSet<Building>() });

            Assert.That(Mask(written, CastleLayout.BuiltBuildingsFromNameEnd), Is.EqualTo(Bits([TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("VillageHall")])));
            Assert.That(Mask(written, CastleLayout.VisibleBuildingsFromNameEnd), Is.EqualTo(0));
        }

        [Test]
        public void ABitOfNoBuildingOfTheFactionIsKept()
        {
            var chunk = Blank();
            var bytes = chunk.ToArray();
            var foreign = 1UL << 63;
            BitConverter.GetBytes(foreign).CopyTo(bytes, NameEnd + CastleLayout.BuiltBuildingsFromNameEnd);
            BitConverter.GetBytes(foreign).CopyTo(bytes, NameEnd + CastleLayout.VisibleBuildingsFromNameEnd);
            BitConverter.GetBytes(foreign).CopyTo(bytes, NameEnd + CastleLayout.EnabledBuildingsFromNameEnd);

            var written = this.Write(chunk.WithBytes(bytes), Rampart());

            Assert.That(Mask(written, CastleLayout.BuiltBuildingsFromNameEnd) & foreign, Is.EqualTo(foreign));
            Assert.That(Mask(written, CastleLayout.VisibleBuildingsFromNameEnd) & foreign, Is.EqualTo(foreign));
            Assert.That(Mask(written, CastleLayout.EnabledBuildingsFromNameEnd) & foreign, Is.EqualTo(foreign));
        }

        [Test]
        public void BuildingTheTownHallOfARampartAlsoSetsItsCompanionBitInTheBuiltAndVisibleMasksOnly()
        {
            var plain = this.Write(Blank(), Rampart());
            var hall = this.Write(Blank(), CastleModels.With(Rampart(), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TownHall")));

            var companion = 1UL << 27;
            Assert.That(Mask(hall, CastleLayout.BuiltBuildingsFromNameEnd) & companion, Is.EqualTo(companion));
            Assert.That(Mask(hall, CastleLayout.VisibleBuildingsFromNameEnd) & companion, Is.EqualTo(companion));
            Assert.That(Mask(hall, CastleLayout.EnabledBuildingsFromNameEnd) & companion, Is.EqualTo(Mask(plain, CastleLayout.EnabledBuildingsFromNameEnd) & companion));
        }

        [Test]
        public void TakingTheCityHallDownClearsItsCompanionBit()
        {
            var chunk = Blank();
            var withCity = this.Write(chunk, CastleModels.With(Rampart(), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TownHall"), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("CityHall")));
            var companion = 1UL << 28;
            Assert.That(Mask(withCity, CastleLayout.BuiltBuildingsFromNameEnd) & companion, Is.EqualTo(companion), "built");

            var without = this.Write(withCity, CastleModels.With(Rampart(), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("TownHall")));

            Assert.That(Mask(without, CastleLayout.BuiltBuildingsFromNameEnd) & companion, Is.EqualTo(0));
            Assert.That(Mask(without, CastleLayout.VisibleBuildingsFromNameEnd) & companion, Is.EqualTo(0));
        }

        [Test]
        public void ACompanionBitIsLeftAloneWhileTheBuildingItFollowsIsNotChanged()
        {
            var chunk = Blank();
            var bytes = chunk.ToArray();
            BitConverter.GetBytes(Bits([TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("VillageHall")]) | (1UL << 29)).CopyTo(bytes, NameEnd + CastleLayout.BuiltBuildingsFromNameEnd);

            var written = this.Write(chunk.WithBytes(bytes), Rampart());

            Assert.That(Mask(written, CastleLayout.BuiltBuildingsFromNameEnd) & (1UL << 29), Is.EqualTo(1UL << 29));
        }

        [Test]
        public void TheFortOfAConfluxHasItsCompanionBitAtTwentyTwoAndTheOthersAtTheirOwn()
        {
            var conflux = this.Write(Blank(), CastleModels.With(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Conflux")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")));
            var bulwark = this.Write(Blank(), CastleModels.With(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Bulwark")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")));
            var castle = this.Write(Blank(), CastleModels.With(CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Castle")), TestCatalog.For(GameVersion.HornOfTheAbyss).Buildings.ById("Fort")));

            Assert.That(Mask(conflux, CastleLayout.BuiltBuildingsFromNameEnd) & (1UL << 22), Is.EqualTo(1UL << 22));
            Assert.That(Mask(bulwark, CastleLayout.BuiltBuildingsFromNameEnd) & (1UL << 27), Is.EqualTo(1UL << 27));
            Assert.That(Mask(castle, CastleLayout.BuiltBuildingsFromNameEnd) & (1UL << 27), Is.EqualTo(0));
        }

        [Test]
        public void ACastleOfNoFactionLeavesTheThreeMasksAsTheyAre()
        {
            var chunk = Blank();
            var bytes = chunk.ToArray();
            BitConverter.GetBytes(0x1234UL).CopyTo(bytes, NameEnd + CastleLayout.BuiltBuildingsFromNameEnd);
            BitConverter.GetBytes(0x5678UL).CopyTo(bytes, NameEnd + CastleLayout.VisibleBuildingsFromNameEnd);
            BitConverter.GetBytes(0x9ABCUL).CopyTo(bytes, NameEnd + CastleLayout.EnabledBuildingsFromNameEnd);

            var written = this.Write(chunk.WithBytes(bytes), CastleModels.Village(TestCatalog.For(GameVersion.HornOfTheAbyss).Factions.ById("Neutral")));

            Assert.That(Mask(written, CastleLayout.BuiltBuildingsFromNameEnd), Is.EqualTo(0x1234UL));
            Assert.That(Mask(written, CastleLayout.VisibleBuildingsFromNameEnd), Is.EqualTo(0x5678UL));
            Assert.That(Mask(written, CastleLayout.EnabledBuildingsFromNameEnd), Is.EqualTo(0x9ABCUL));
        }

        [Test]
        public void TheSourceChunkIsNotChangedAndTheSizeIsKept()
        {
            var chunk = Blank();
            var before = chunk.ToArray();

            var written = this.Write(chunk, Rampart() with { GarrisonedHeroId = 3, MageGuildSpellCounts = Numbers.Of(1, 1, 1, 1, 1) });

            Assert.That(chunk.ToArray(), Is.EqualTo(before));
            Assert.That(written.Length, Is.EqualTo(chunk.Length));
        }
    }
}
