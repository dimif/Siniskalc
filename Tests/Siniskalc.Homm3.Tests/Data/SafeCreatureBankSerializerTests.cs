using Siniskalc.Core.Data;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests, on records of creature banks made by hand, which bytes the writer of a bank changes: the guards, the rewards and the artifacts.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SafeCreatureBankSerializerTests
    {
        private readonly SafeCreatureBankSerializer serializer = new(CreatureBankRecords.Catalog);

        private static int[] Ints(CreatureBankChunk chunk, int offset) => Enumerable.Range(0, Constants.CreatureSlotCount).Select(i => chunk.GetInt32At(offset + (i * 4))).ToArray();

        private static int[] Types(CreatureBankChunk chunk) => Ints(chunk, 0);

        private static int[] Counts(CreatureBankChunk chunk) => Ints(chunk, CreatureBankLayout.Counts);

        private static int[] Resources(CreatureBankChunk chunk) => Ints(chunk, CreatureBankLayout.Resources);

        private static uint[] Artifacts(CreatureBankChunk chunk) => Enumerable.Range(0, Constants.CreatureBank.ArtifactSlots).Select(i => chunk.GetUInt32At(CreatureBankLayout.Artifacts + (i * 4))).ToArray();

        private CreatureBankChunk Write(CreatureBankChunk chunk, CreatureBankModel model) => this.serializer.Serialize(chunk, model);

        [Test]
        public void AModelThatSaysWhatTheRecordHoldsLeavesEveryByteAlone()
        {
            var chunk = CreatureBankRecords.Chunk(CreatureBankKind.DragonUtopia, 3, artifacts: [CreatureBankRecords.Catalog.WearableArtifacts[0].Code, uint.MaxValue, uint.MaxValue, uint.MaxValue]);

            var written = this.Write(chunk, CreatureBankRecords.Model(CreatureBankKind.DragonUtopia, 3, artifacts: [CreatureBankRecords.Catalog.WearableArtifacts[0], null, null, null]));

            Assert.That(written.ToArray(), Is.EqualTo(chunk.ToArray()));
        }

        [Test]
        public void ANewStrengthWritesTheGuardCountsAndTheRewardsOfThatTier()
        {
            var written = this.Write(CreatureBankRecords.Chunk(CreatureBankKind.CyclopsStockpile, 1), CreatureBankRecords.Model(CreatureBankKind.CyclopsStockpile, 3));

            Assert.That(Types(written), Is.EqualTo(new[] { 94, 94, 94, 94, 94, -1, -1 }));
            Assert.That(Counts(written), Is.EqualTo(new[] { 8, 8, 8, 8, 8, 0, 0 }));
            Assert.That(Resources(written), Is.EqualTo(new[] { 8, 8, 8, 8, 8, 8, 0 }));
        }

        [Test]
        public void ANewStrengthOfADwarvenTreasuryWritesItsCrystalAndGoldPayout()
        {
            var written = this.Write(CreatureBankRecords.Chunk(CreatureBankKind.DwarvenTreasury, 1), CreatureBankRecords.Model(CreatureBankKind.DwarvenTreasury, 4));

            Assert.That(Counts(written), Is.EqualTo(new[] { 30, 30, 30, 30, 30, 0, 0 }));
            Assert.That(Resources(written), Is.EqualTo(new[] { 0, 0, 0, 0, 10, 0, 7500 }));
        }

        [Test]
        public void ANewStrengthOfADragonUtopiaWritesItsCountsAndItsGold()
        {
            var written = this.Write(CreatureBankRecords.Chunk(CreatureBankKind.DragonUtopia, 1), CreatureBankRecords.Model(CreatureBankKind.DragonUtopia, 4));

            Assert.That(Types(written), Is.EqualTo(new[] { 26, 82, 27, 83, -1, -1, -1 }));
            Assert.That(Counts(written), Is.EqualTo(new[] { 8, 7, 6, 5, 0, 0, 0 }));
            Assert.That(Resources(written), Is.EqualTo(new[] { 0, 0, 0, 0, 0, 0, 50000 }));
        }

        [Test]
        public void ChangingOnlyTheStrengthChangesNothingButTheCountsAndTheRewards()
        {
            var chunk = CreatureBankRecords.Chunk(CreatureBankKind.ImpCache, 1);

            var written = this.Write(chunk, CreatureBankRecords.Model(CreatureBankKind.ImpCache, 2));

            var changed = ByteDiff.Of(chunk.ToArray(), written.ToArray()).Select(d => d.Offset).ToList();
            Assert.That(changed, Is.All.InRange(CreatureBankLayout.Counts, CreatureBankLayout.Resources + (Constants.CreatureBank.ResourceCount * 4) - 1));
            Assert.That(changed, Does.Contain(CreatureBankLayout.Counts).And.Contain(CreatureBankLayout.Resources + 4));
        }

        [Test]
        public void ClearingABankWritesMinusOneGuardsAndNoCountsAndKeepsTheRewards()
        {
            var chunk = CreatureBankRecords.Chunk(CreatureBankKind.MedusaStores, 2);

            var written = this.Write(chunk, CreatureBankRecords.Model(CreatureBankKind.MedusaStores, 2, cleared: true));

            Assert.That(Types(written), Is.All.EqualTo(-1));
            Assert.That(Counts(written), Is.All.EqualTo(0));
            Assert.That(Resources(written), Is.EqualTo(Resources(chunk)));
        }

        [Test]
        public void RestoringAClearedBankWritesItsGuardsAgain()
        {
            var cleared = CreatureBankRecords.Chunk(CreatureBankKind.NagaBank, 3, cleared: true);

            var written = this.Write(cleared, CreatureBankRecords.Model(CreatureBankKind.NagaBank, 3));

            Assert.That(Types(written), Is.EqualTo(new[] { 38, 38, 38, 38, 38, -1, -1 }));
            Assert.That(Counts(written), Is.EqualTo(new[] { 4, 4, 4, 4, 4, 0, 0 }));
        }

        [Test]
        public void UpgradingTheCentreChangesOnlyTheTypeOfTheCentreGuard()
        {
            var chunk = CreatureBankRecords.Chunk(CreatureBankKind.CyclopsStockpile, 2);

            var written = this.Write(chunk, CreatureBankRecords.Model(CreatureBankKind.CyclopsStockpile, 2, upgraded: true));

            Assert.That(ByteDiff.Of(chunk.ToArray(), written.ToArray()), Is.EqualTo(new[] { (Constants.CreatureBank.CenterSlot * 4, (byte)94, (byte)95) }));
        }

        [Test]
        public void GuardBytesThatAreNoPresetAreKeptWhileTheModelDoesNotChangeTheGuards()
        {
            var odd = CreatureBankRecords.WithBytes(CreatureBankRecords.Chunk(CreatureBankKind.ImpCache, 2), (CreatureBankLayout.Counts, 99), (0, 77));

            var written = this.Write(odd, CreatureBankRecords.Model(CreatureBankKind.ImpCache, 2));

            Assert.That(written.ToArray(), Is.EqualTo(odd.ToArray()));
        }

        [Test]
        public void RewardsThatMatchNoTierAreKeptWhileTheModelStillHoldsTheStandInTierAndWrittenOnceItChanges()
        {
            var odd = CreatureBankRecords.WithBytes(CreatureBankRecords.Chunk(CreatureBankKind.DwarvenTreasury, 1), (CreatureBankLayout.Resources + 16, 0x77));

            var kept = this.Write(odd, CreatureBankRecords.Model(CreatureBankKind.DwarvenTreasury, Constants.CreatureBank.MinStrength));
            var replaced = this.Write(odd, CreatureBankRecords.Model(CreatureBankKind.DwarvenTreasury, 2));

            Assert.That(Resources(kept), Is.EqualTo(Resources(odd)));
            Assert.That(Resources(replaced), Is.EqualTo(new[] { 0, 0, 0, 0, 3, 0, 4000 }));
        }

        [Test]
        public void AnArtifactCodeThatIsNoWearableArtifactIsKeptWhileTheModelStillHoldsTheFallbackArtifact()
        {
            var chunk = CreatureBankRecords.Chunk(CreatureBankKind.DragonUtopia, artifacts: [CreatureBankRecords.UnknownArtifactCode, 0x1234u, uint.MaxValue, uint.MaxValue]);
            var fallback = CreatureBankRecords.Catalog.FallbackArtifact;

            var written = this.Write(chunk, CreatureBankRecords.Model(CreatureBankKind.DragonUtopia, artifacts: [fallback, fallback, null, null]));

            Assert.That(Artifacts(written), Is.EqualTo(new[] { CreatureBankRecords.UnknownArtifactCode, 0x1234u, uint.MaxValue, uint.MaxValue }));
        }

        [Test]
        public void AnArtifactCodeThatIsNoWearableArtifactIsReplacedAsSoonAsTheModelHoldsAnotherArtifactOrNone()
        {
            var chunk = CreatureBankRecords.Chunk(CreatureBankKind.DragonUtopia, artifacts: [CreatureBankRecords.UnknownArtifactCode, 0x1234u, uint.MaxValue, uint.MaxValue]);
            var other = CreatureBankRecords.Catalog.WearableArtifacts[1];

            var written = this.Write(chunk, CreatureBankRecords.Model(CreatureBankKind.DragonUtopia, artifacts: [other, null, null, null]));

            Assert.That(Artifacts(written), Is.EqualTo(new[] { (uint)other.Code, uint.MaxValue, uint.MaxValue, uint.MaxValue }));
        }

        [Test]
        public void AnArtifactPutInAnEmptySlotIsWrittenAsItsCodeInFourBytes()
        {
            var chunk = CreatureBankRecords.Chunk(CreatureBankKind.DragonUtopia);
            var artifact = CreatureBankRecords.Catalog.WearableArtifacts[2];

            var written = this.Write(chunk, CreatureBankRecords.Model(CreatureBankKind.DragonUtopia, artifacts: [null, null, null, artifact]));

            Assert.That(written.GetBytesAt(CreatureBankLayout.Artifacts + 12, 4), Is.EqualTo(new[] { artifact.Code, (byte)0, (byte)0, (byte)0 }));
            Assert.That(ByteDiff.Of(chunk.ToArray(), written.ToArray()).Select(d => d.Offset), Is.All.InRange(CreatureBankLayout.Artifacts + 12, CreatureBankLayout.Artifacts + 15));
        }

        [Test]
        public void RemovingAnArtifactWritesFFFFFFFFInItsSlot()
        {
            var artifact = CreatureBankRecords.Catalog.WearableArtifacts[0];
            var chunk = CreatureBankRecords.Chunk(CreatureBankKind.DragonUtopia, artifacts: [uint.MaxValue, artifact.Code, uint.MaxValue, uint.MaxValue]);

            var written = this.Write(chunk, CreatureBankRecords.Model(CreatureBankKind.DragonUtopia));

            Assert.That(written.GetBytesAt(CreatureBankLayout.Artifacts + 4, 4), Is.EqualTo(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }));
        }

        [Test]
        public void ABankOfAnotherKindThanTheRecordIsRefused()
        {
            Assert.That(() => this.Write(CreatureBankRecords.Chunk(CreatureBankKind.ImpCache), CreatureBankRecords.Model(CreatureBankKind.NagaBank)), Throws.ArgumentException);
        }

        [TestCase(0)]
        [TestCase(5)]
        public void AStrengthOutsideTheTiersIsRefused(int strength)
        {
            Assert.That(() => this.Write(CreatureBankRecords.Chunk(CreatureBankKind.ImpCache), CreatureBankRecords.Model(CreatureBankKind.ImpCache, strength)), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ADragonUtopiaWithAnotherNumberOfArtifactSlotsThanFourIsRefused()
        {
            Assert.That(() => this.Write(CreatureBankRecords.Chunk(CreatureBankKind.DragonUtopia), CreatureBankRecords.Model(CreatureBankKind.DragonUtopia, artifacts: new Artifact?[3])), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ABankOtherThanADragonUtopiaWithArtifactsIsRefused()
        {
            Assert.That(() => this.Write(CreatureBankRecords.Chunk(CreatureBankKind.ImpCache), CreatureBankRecords.Model(CreatureBankKind.ImpCache, artifacts: new Artifact?[1])), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TheSourceRecordIsNotChangedAndTheEntranceAndSizeAreKept()
        {
            var chunk = CreatureBankRecords.Chunk(CreatureBankKind.ImpCache, 1, entrance: 0x201F, occupyingHero: 4);
            var before = chunk.ToArray();

            var written = this.Write(chunk, CreatureBankRecords.Model(CreatureBankKind.ImpCache, 3, cleared: true));

            Assert.That(chunk.ToArray(), Is.EqualTo(before));
            Assert.That(written.Length, Is.EqualTo(CreatureBankRecords.Size));
            Assert.That((written.Entrance.GetUInt32At(0), written.OccupyingHero, written.BankIndex, written.Position), Is.EqualTo((0x201Fu, (int?)4, 3, CreatureBankRecords.Position)));
        }

        [Test]
        public void ARecordThatIsTooShortToHoldTheRewardsIsRefusedAsOutOfRange()
        {
            var chunk = new CreatureBankChunk("Bank", new byte[10], 0, 0, 0, CreatureBankRecords.Position, CreatureBankKind.NagaBank, new RawBytesChunk("Entrance", new byte[4], 20), null);

            Assert.That(() => this.Write(chunk, CreatureBankRecords.Model(CreatureBankKind.NagaBank)), Throws.InstanceOf<ArgumentException>());
        }
    }
}
