namespace Siniskalc.Core.Tests.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Models;

    /// <summary>
    /// Tests the collecting of the pending changes a differ finds.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ChangeSinkTests
    {
        private static readonly ChangeArea Area = new("General");

        [Test]
        public void AChangeAddedIsMadeIntoAPendingChangeBySuppliedFactory()
        {
            var sink = new ChangeSink<TestModel>((change, revert) => new PendingChange("Thing", change, TestChunkKinds.Unit, 3, _ => { }));

            sink.Add(Area, "Value", "1", "2", m => m with { Value = 1 });

            Assert.That(sink.All, Has.Count.EqualTo(1));
            Assert.That(sink.All[0].Change, Is.EqualTo(new Change(Area, "Value", "1", "2")));
            Assert.That((sink.All[0].Kind, sink.All[0].Index), Is.EqualTo((TestChunkKinds.Unit, 3)));
        }

        [Test]
        public void TheChangesComeInTheOrderTheyWereAdded()
        {
            var sink = new ChangeSink<TestModel>((change, _) => new PendingChange("Thing", change, TestChunkKinds.Unit, 0, _ => { }));

            sink.Add(Area, "First", "a", "b", m => m);
            sink.Add(Area, "Second", "a", "b", m => m);

            Assert.That(sink.All.Select(c => c.Change.Description), Is.EqualTo(new[] { "First", "Second" }));
        }
    }
}
