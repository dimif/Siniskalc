using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Heroes.Data;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Data;

namespace Siniskalc.Homm3.Tests.MultiUnit.Data
{
    /// <summary>
    /// How the entrance of a creature bank follows its guards when a bank is cleared or restored, on documents made by hand: the document
    /// factory and the reader of the banks are real, everything else is a fake.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class CreatureBankReconcilerTests
    {
        // The state of an entrance that the game has not touched: the number of the bank in bits 13 and up, and the bits of a bank that waits for a hero.
        private const uint Ready = 0x201F;
        private const uint ClearedState = 0x0200203F;

        private Mock<IDocumentReconciler> playerLists = null!;
        private Mock<ICreatureBankLoader> loader = null!;
        private SaveDocumentFactory factory = null!;
        private CreatureBankReconciler reconciler = null!;

        [SetUp]
        public void SetUp()
        {
            this.playerLists = new Mock<IDocumentReconciler>();
            this.playerLists
                .Setup(r => r.Reconcile(It.IsAny<SaveDocument>(), It.IsAny<SaveDocument>(), It.IsAny<SaveDocument>()))
                .Returns((SaveDocument _, SaveDocument _, SaveDocument after) => after);
            this.loader = new Mock<ICreatureBankLoader>();
            this.reconciler = new CreatureBankReconciler(this.playerLists.Object);
            this.factory = new SaveDocumentFactory(
                new VersionSpecificEntitiesProviderFactory(TestCatalog.Catalogs),
                Mock.Of<IDiffer<HeroModel>>(),
                _ => Mock.Of<ICastleDeserializer>(),
                Mock.Of<ISerializer<CastleChunk, CastleModel>>(),
                Mock.Of<ICastleDiffer>(),
                Mock.Of<IPlayerDeserializer>(),
                Mock.Of<ISerializer<PlayerChunk, PlayerModel>>(),
                Mock.Of<IDeserializer<PlayerChunk, PlayerLists>>(),
                Mock.Of<ISerializer<PlayerChunk, PlayerLists>>(),
                Mock.Of<IPlayerDiffer>(),
                Mock.Of<IDeserializer<ObeliskChunk, ObeliskVisits<PlayerColor>>>(),
                Mock.Of<ISerializer<ObeliskChunk, ObeliskVisits<PlayerColor>>>(),
                Mock.Of<IDiffer<ObeliskVisits<PlayerColor>>>(),
                _ => new RecordKit<RecordChunk, MineModel>(Mock.Of<IDeserializer<RecordChunk, MineModel>>(), Mock.Of<ISerializer<RecordChunk, MineModel>>(), Mock.Of<IDiffer<MineModel>>()),
                _ => new RecordKit<RecordChunk, DwellingModel>(Mock.Of<IDeserializer<RecordChunk, DwellingModel>>(), Mock.Of<ISerializer<RecordChunk, DwellingModel>>(), Mock.Of<IDiffer<DwellingModel>>()),
                _ => new RecordKit<RecordChunk, MerchantStockModel>(Mock.Of<IDeserializer<RecordChunk, MerchantStockModel>>(), Mock.Of<ISerializer<RecordChunk, MerchantStockModel>>(), Mock.Of<IDiffer<MerchantStockModel>>()),
                _ => new RecordKit<RecordChunk, MerchantStockModel>(Mock.Of<IDeserializer<RecordChunk, MerchantStockModel>>(), Mock.Of<ISerializer<RecordChunk, MerchantStockModel>>(), Mock.Of<IDiffer<MerchantStockModel>>()),
                new RecordKit<RecordChunk, HeroOwnersModel>(Mock.Of<IDeserializer<RecordChunk, HeroOwnersModel>>(), Mock.Of<ISerializer<RecordChunk, HeroOwnersModel>>(), Mock.Of<IDiffer<HeroOwnersModel>>()),
                new RecordKit<RecordChunk, TurnModel>(Mock.Of<IDeserializer<RecordChunk, TurnModel>>(), Mock.Of<ISerializer<RecordChunk, TurnModel>>(), Mock.Of<IDiffer<TurnModel>>()),
                Mock.Of<IDocumentReconciler>(),
                Mock.Of<ISavefileBytesPatcher>(),
                this.loader.Object,
                catalog => new RecordKit<CreatureBankChunk, CreatureBankModel>(new CreatureBankDeserializer(catalog), Mock.Of<ISerializer<CreatureBankChunk, CreatureBankModel>>(), Mock.Of<IDiffer<CreatureBankModel>>()));
        }

        private static CreatureBankChunk Bank(bool cleared, uint entrance, int? occupyingHero = null) =>
            CreatureBankRecords.Chunk(CreatureBankKind.ImpCache, 2, cleared, entrance: entrance, occupyingHero: occupyingHero);

        private static HeroChunk Hero(uint state)
        {
            var bytes = new byte[1200];
            BitConverter.GetBytes(state).CopyTo(bytes, CreatureBankLayout.UnderHeroState);
            return new HeroChunk("Orrin", bytes, 100);
        }

        private SaveDocument Document(CreatureBankChunk bank, params HeroChunk[] heroes)
        {
            this.loader
                .Setup(l => l.Load(It.IsAny<byte[]>(), It.IsAny<GameVersion>(), It.IsAny<IReadOnlyList<HeroChunk>>(), It.IsAny<int>()))
                .Returns([bank]);
            return this.factory.Create(new byte[400], heroes, new HeroOffsets(GameVersion.ShadowOfDeath));
        }

        private static uint Entrance(SaveDocument document) => document.CreatureBanks()[0].Entrance.GetUInt32At(0);

        private static uint HeroState(SaveDocument document) => CreatureBankLayout.HeroValue(document.Heroes()[0], CreatureBankLayout.UnderHeroState);

        private static SaveDocument Edited(SaveDocument document, CreatureBankChunk bank) => document.WithChunk(Homm3ChunkKinds.CreatureBanks, 0, bank);

        [Test]
        public void ABankThatWasNotEditedKeepsItsEntranceAndTheDocumentTheOtherReconcilerMade()
        {
            var original = this.Document(Bank(false, Ready));
            var after = Edited(original, Bank(false, Ready));

            var reconciled = this.reconciler.Reconcile(original, original, after);

            Assert.That(reconciled, Is.SameAs(after));
            Assert.That(Entrance(reconciled), Is.EqualTo(Ready));
        }

        [Test]
        public void TheDocumentIsFirstReconciledByTheReconcilerOfThePlayerLists()
        {
            var original = this.Document(Bank(false, Ready));
            var after = Edited(original, Bank(true, Ready));
            var madeByPlayerLists = Edited(original, Bank(true, Ready));
            this.playerLists.Setup(r => r.Reconcile(original, original, after)).Returns(madeByPlayerLists);

            var reconciled = this.reconciler.Reconcile(original, original, after);

            this.playerLists.Verify(r => r.Reconcile(original, original, after), Times.Once);
            Assert.That(reconciled.CreatureBanks()[0], Is.Not.SameAs(after.CreatureBanks()[0]), "the entrance was written to the chunk the other reconciler returned");
        }

        [Test]
        public void ClearingABankSetsTheClearedBitsOfItsEntranceAndKeepsTheOthers()
        {
            var original = this.Document(Bank(false, Ready));
            var after = Edited(original, Bank(true, Ready));

            var reconciled = this.reconciler.Reconcile(original, original, after);

            Assert.That(Entrance(reconciled), Is.EqualTo(ClearedState));
            Assert.That(reconciled.CreatureBanks()[0].ToArray(), Is.EqualTo(after.CreatureBanks()[0].ToArray()), "the guards are not touched");
        }

        [Test]
        public void RestoringTheGuardsOfABankTheGameClearedSetsTheReadyBitsAndKeepsTheNumberOfTheBank()
        {
            var original = this.Document(Bank(true, ClearedState));
            var before = original;
            var after = Edited(original, Bank(false, ClearedState));

            var reconciled = this.reconciler.Reconcile(original, before, after);

            Assert.That(Entrance(reconciled), Is.EqualTo(Ready));
        }

        [Test]
        public void PuttingBackTheGuardsOfABankClearedByHandGivesTheEntranceItHadInTheOriginal()
        {
            var original = this.Document(Bank(false, 0x201F));
            var cleared = Edited(original, Bank(true, ClearedState));
            var after = Edited(original, Bank(false, ClearedState));

            var reconciled = this.reconciler.Reconcile(original, cleared, after);

            Assert.That(Entrance(reconciled), Is.EqualTo(0x201Fu));
        }

        [Test]
        public void BitsOfTheEntranceOutsideTheResetMaskAreKept()
        {
            var original = this.Document(Bank(false, 0x201F | 0x0100_0000));
            var after = Edited(original, Bank(true, 0x201F | 0x0100_0000));

            var reconciled = this.reconciler.Reconcile(original, original, after);

            Assert.That(Entrance(reconciled), Is.EqualTo(0x0300_203Fu));
        }

        [Test]
        public void ClearingABankWithAHeroOnItSetsTheClearedBitsInTheStateOfTheHeroAndNotInTheEntrance()
        {
            var original = this.Document(Bank(false, 0, occupyingHero: 0), Hero(Ready));
            var after = Edited(original, Bank(true, 0, occupyingHero: 0));

            var reconciled = this.reconciler.Reconcile(original, original, after);

            Assert.That(HeroState(reconciled), Is.EqualTo(ClearedState));
            Assert.That(Entrance(reconciled), Is.EqualTo(0u));
        }

        [Test]
        public void RestoringTheGuardsOfABankWithAHeroOnItSetsTheReadyBitsInTheStateOfTheHero()
        {
            var original = this.Document(Bank(true, 0, occupyingHero: 0), Hero(ClearedState));
            var after = Edited(original, Bank(false, 0, occupyingHero: 0));

            var reconciled = this.reconciler.Reconcile(original, original, after);

            Assert.That(HeroState(reconciled), Is.EqualTo(Ready));
        }
    }
}
