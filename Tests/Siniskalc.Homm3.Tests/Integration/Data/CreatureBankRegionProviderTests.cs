namespace Siniskalc.Homm3.Tests.Integration.Data
{
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Tests.Data;

    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CreatureBankRegionProviderTests
    {
        [TestCase(0, 107, true)]
        [TestCase(8, 91, false)]
        [TestCase(12, 91, false)]
        [TestCase(16, 91, false)]
        [TestCase(20, 91, false)]
        [TestCase(24, 91, false)]
        public void TheRegionsCoverEachBankRecordAndNameOnlyTheFieldsItContains(int index, int length, bool hasArtifacts)
        {
            var path = Path.Combine("TestData", "Hota", "CreatureBanks", "creature_banks_00_baseline.GM2");
            var game = SavedGames.Get(path);
            using var target = TestServices.ByteEditTargets.ForCreatureBank(game.History, index);
            var names = new[]
            {
                "Guard creatures", "Guard counts", "Resources", "Reward creature", "Reward creature count", "Artifact count", "Artifacts",
            };
            Assert.That(target.Regions.Select(r => r.Name), Is.EqualTo(hasArtifacts ? names : names[..^1]));
            Assert.That(target.Regions.SelectMany(r => Enumerable.Range(r.Start, r.Length)), Is.EqualTo(Enumerable.Range(0, length)));
            Assert.That(target.GetBytes(), Has.Length.EqualTo(length));
        }
    }
}
