using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// Tests how the record of a creature bank is read, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class CreatureBankDeserializerTests
    {
        private readonly CreatureBankDeserializer deserializer = new(CreatureBankRecords.Catalog);

        private static readonly CreatureBankKind[] Kinds = Enum.GetValues<CreatureBankKind>();

        [Test]
        public void TheNamePositionAndKindAreThoseOfTheRecord()
        {
            var model = this.deserializer.Deserialize(CreatureBankRecords.Chunk(CreatureBankKind.MedusaStores));

            Assert.That((model.Name, model.Position, model.Kind), Is.EqualTo(("MedusaStores 3", CreatureBankRecords.Position, CreatureBankKind.MedusaStores)));
        }

        [Test]
        public void TheStrengthIsTheTierWhoseRewardsTheRecordHolds([ValueSource(nameof(Kinds))] CreatureBankKind kind, [Values(1, 2, 3, 4)] int strength)
        {
            var model = this.deserializer.Deserialize(CreatureBankRecords.Chunk(kind, strength));

            Assert.That(model.Strength, Is.EqualTo(strength));
        }

        [Test]
        public void RewardsThatMatchNoTierReadAsTheWeakestTierWithoutThrowing()
        {
            var chunk = CreatureBankRecords.WithBytes(CreatureBankRecords.Chunk(CreatureBankKind.DwarvenTreasury, 3), (CreatureBankLayout.Resources + 16, 0x77));

            Assert.That(this.deserializer.Deserialize(chunk).Strength, Is.EqualTo(Constants.CreatureBank.MinStrength));
        }

        [Test]
        public void ARecordWhoseFirstGuardTypeIsMinusOneIsCleared()
        {
            var cleared = this.deserializer.Deserialize(CreatureBankRecords.Chunk(CreatureBankKind.ImpCache, 2, cleared: true));
            var guarded = this.deserializer.Deserialize(CreatureBankRecords.Chunk(CreatureBankKind.ImpCache, 2));

            Assert.That((cleared.IsCleared, guarded.IsCleared), Is.EqualTo((true, false)));
        }

        [Test]
        public void TheCentreIsUpgradedWhenItsGuardIsTheUpgradeOfTheOthers()
        {
            var upgraded = this.deserializer.Deserialize(CreatureBankRecords.Chunk(CreatureBankKind.CyclopsStockpile, upgraded: true));
            var plain = this.deserializer.Deserialize(CreatureBankRecords.Chunk(CreatureBankKind.CyclopsStockpile));

            Assert.That((upgraded.UpgradedCenter, plain.UpgradedCenter), Is.EqualTo((true, false)));
        }

        [Test]
        public void ADragonUtopiaHasNoUpgradedCentre()
        {
            Assert.That(this.deserializer.Deserialize(CreatureBankRecords.Chunk(CreatureBankKind.DragonUtopia)).UpgradedCenter, Is.False);
        }

        [Test]
        public void ABankOtherThanADragonUtopiaHasNoArtifacts()
        {
            Assert.That(this.deserializer.Deserialize(CreatureBankRecords.Chunk(CreatureBankKind.NagaBank)).Artifacts, Is.Empty);
        }

        [Test]
        public void TheArtifactsOfADragonUtopiaAreReadFromTheirCodesAndFFFFFFFFIsNone()
        {
            var first = CreatureBankRecords.Catalog.WearableArtifacts[0];
            var second = CreatureBankRecords.Catalog.WearableArtifacts[1];

            var model = this.deserializer.Deserialize(CreatureBankRecords.Chunk(CreatureBankKind.DragonUtopia, artifacts: [first.Code, uint.MaxValue, second.Code, uint.MaxValue]));

            Assert.That(model.Artifacts, Is.EqualTo(new[] { first, null, second, null }));
        }

        [Test]
        public void AnArtifactCodeThatIsNoWearableArtifactReadsAsTheFallbackArtifact()
        {
            var model = this.deserializer.Deserialize(CreatureBankRecords.Chunk(CreatureBankKind.DragonUtopia, artifacts: [CreatureBankRecords.UnknownArtifactCode, 0x1234u, 0x80000000u, uint.MaxValue]));

            Assert.That(model.Artifacts, Is.EqualTo(new[] { CreatureBankRecords.Catalog.FallbackArtifact, CreatureBankRecords.Catalog.FallbackArtifact, CreatureBankRecords.Catalog.FallbackArtifact, null }));
        }

        [Test]
        public void ARecordThatIsTooShortToHoldTheRewardsIsRefusedAsOutOfRange()
        {
            var chunk = new CreatureBankChunk("Bank", new byte[10], 0, 0, 0, CreatureBankRecords.Position, CreatureBankKind.NagaBank, new Siniskalc.Core.Data.RawBytesChunk("Entrance", new byte[4], 20), null);

            Assert.That(() => this.deserializer.Deserialize(chunk), Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void ADragonUtopiaRecordThatEndsBeforeItsArtifactsIsRefusedAsOutOfRange()
        {
            var bytes = CreatureBankRecords.Chunk(CreatureBankKind.DragonUtopia).ToArray()[..(CreatureBankLayout.Artifacts + 6)];
            var chunk = new CreatureBankChunk("Bank", bytes, 0, 0, 0, CreatureBankRecords.Position, CreatureBankKind.DragonUtopia, new Siniskalc.Core.Data.RawBytesChunk("Entrance", new byte[4], 200), null);

            Assert.That(() => this.deserializer.Deserialize(chunk), Throws.InstanceOf<ArgumentException>());
        }
    }
}
