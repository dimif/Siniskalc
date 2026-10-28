namespace Siniskalc.Homm2.Tests.Data
{
    using Siniskalc.Core.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What the differ of the castles that built something this turn lists, on records made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class BuiltTodayDifferTests
    {
        private readonly BuiltTodayDiffer differ = new();

        [Test]
        public void TwoEqualListsHaveNoChanges()
        {
            Assert.That(this.differ.Diff(0, new BuiltTodayModel(0, Numbers.Of(3, 5)), new BuiltTodayModel(0, Numbers.Of(3, 5))), Is.Empty);
        }

        [Test]
        public void EachCastleThatGainedOrLostTheMarkIsOneChangeInTheOrderOfTheCastles()
        {
            var original = new BuiltTodayModel(0, Numbers.Of(5, 9));
            var current = new BuiltTodayModel(0, Numbers.Of(3, 9));

            var changes = this.differ.Diff(2, original, current);

            Assert.That(changes.Select(c => (c.Change.Description, c.Change.Before, c.Change.After)), Is.EqualTo(new[]
            {
                ("Built this turn (castle 3)", "No", "Yes"),
                ("Built this turn (castle 5)", "Yes", "No"),
            }));
            Assert.That(changes.Select(c => (c.OwnerName, c.Kind, c.Index)).Distinct(), Is.EqualTo(new[] { ("Built this turn", Homm2ChunkKinds.BuiltToday, 2) }));
        }
    }
}
