namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What the differ of the sphinxes of the map lists, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SphinxDifferTests
    {
        private static SphinxModel Sphinx() => new(0, new MapPosition(5, 6), true, [0, 0, 0, 0, 0, 0, 1000], SphinxModel.NoArtifact, ["Time", "Clock"], "What is it?", 100, TestCatalog.Standard);

        [Test]
        public void TwoEqualSphinxesHaveNoChanges()
        {
            Assert.That(new SphinxDiffer().Diff(0, Sphinx(), Sphinx()), Is.Empty);
        }

        [Test]
        public void ASphinxListsTheRiddleAnsweredEachRewardTheAnswersAndTheText()
        {
            var original = Sphinx();
            var current = original with
            {
                IsActive = false,
                Reward = [0, 0, 5, 0, 0, 0, 2500],
                ArtifactCode = 3,
                Answers = ["Time"],
                Riddle = "What?",
            };

            var changes = new SphinxDiffer().Diff(0, original, current);

            Assert.That(changes.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[]
            {
                ("Riddle", "not answered", "answered"),
                ("Reward: Ore", "0", "5"),
                ("Reward: Gold", "1,000", "2,500"),
                ("Reward: artifact", "96", "3"),
                ("Answers", "Time, Clock", "Time"),
                ("Riddle text", "What is it?", "What?"),
            }));
            Assert.That(changes.Select(c => c.Kind).Distinct(), Is.EqualTo(new[] { Homm2ChunkKinds.Sphinxes }));
        }
    }
}
