using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;

namespace Siniskalc.Homm3.Tests.Data
{
    /// <summary>
    /// What the differ of the creature banks lists, on models made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class CreatureBankDifferTests
    {
        private static readonly CreatureBankDiffer Differ = new();

        private static (string Description, string Before, string After) Text(Siniskalc.Core.Data.PendingChange change) => (change.Change.Description, change.Change.Before, change.Change.After);

        private static CreatureBankModel Utopia(params Artifact?[] artifacts) => CreatureBankRecords.Model(CreatureBankKind.DragonUtopia, artifacts: artifacts.Length == 0 ? null : artifacts);

        [Test]
        public void TwoEqualBanksHaveNoChanges()
        {
            Assert.That(Differ.Diff(0, Utopia(), Utopia()), Is.Empty);
        }

        [Test]
        public void AClearedBankIsOneChangeOfThePlaceNamedByTheBank()
        {
            var original = CreatureBankRecords.Model(CreatureBankKind.ImpCache);

            var change = Differ.Diff(2, original, original with { IsCleared = true }).Single();

            Assert.That(Text(change), Is.EqualTo(("Cleared", "No", "Yes")));
            Assert.That((change.OwnerName, change.Kind, change.Index), Is.EqualTo((original.Name, Homm3ChunkKinds.CreatureBanks, 2)));
        }

        [Test]
        public void ANewStrengthIsOneChangeOfTheGuardsAndTheRewards()
        {
            var original = CreatureBankRecords.Model(CreatureBankKind.ImpCache, 1);

            var change = Differ.Diff(0, original, original with { Strength = 4 }).Single();

            Assert.That(Text(change), Is.EqualTo(("Guard strength and rewards", "1", "4")));
        }

        [Test]
        public void AnUpgradedCentreIsOneChange()
        {
            var original = CreatureBankRecords.Model(CreatureBankKind.NagaBank);

            var change = Differ.Diff(0, original, original with { UpgradedCenter = true }).Single();

            Assert.That(Text(change), Is.EqualTo(("Upgraded centre", "No", "Yes")));
        }

        [Test]
        public void AnUpgradedCentreOfABankThatIsClearedIsNotListedOnItsOwn()
        {
            var original = CreatureBankRecords.Model(CreatureBankKind.NagaBank);

            var changes = Differ.Diff(0, original, original with { IsCleared = true, UpgradedCenter = true });

            Assert.That(changes.Select(Text), Is.EqualTo(new[] { ("Cleared", "No", "Yes") }));
        }

        [Test]
        public void AChangedArtifactIsOneChangeNumberedFromOneAndNamedByTheArtifactOrNone()
        {
            var artifact = CreatureBankRecords.Catalog.WearableArtifacts[0];

            var changes = Differ.Diff(0, Utopia(), Utopia(null, artifact, null, null));

            Assert.That(changes.Select(Text), Is.EqualTo(new[] { ("Artifact 2", "(none)", artifact.Name) }));
        }

        [Test]
        public void SeveralEditsAreSeveralChangesInTheOrderOfTheModel()
        {
            var artifact = CreatureBankRecords.Catalog.WearableArtifacts[0];
            var original = Utopia();

            var changes = Differ.Diff(0, original, original with { Strength = 2, Artifacts = [null, null, null, artifact] });

            Assert.That(changes.Select(c => c.Change.Description), Is.EqualTo(new[] { "Guard strength and rewards", "Artifact 4" }));
        }
    }
}
