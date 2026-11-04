namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the list of records a Hex tab shows and the one that is picked.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ByteRecordListViewModelTests
    {
        [Test]
        public void TheFirstRecordIsPickedAndAnEmptyListPicksNone()
        {
            var first = new ByteRecordViewModel("A", () => new Mock<IByteEditTarget>().Object);
            var second = new ByteRecordViewModel("B", () => new Mock<IByteEditTarget>().Object);

            Assert.That(new ByteRecordListViewModel(new[] { first, second }).SelectedRecord, Is.SameAs(first));
            Assert.That(new ByteRecordListViewModel(new List<ByteRecordViewModel>()).SelectedRecord, Is.Null);
        }

        [Test]
        public void PickingARecordRaisesPropertyChangedOnlyWhenItChanges()
        {
            var first = new ByteRecordViewModel("A", () => new Mock<IByteEditTarget>().Object);
            var second = new ByteRecordViewModel("B", () => new Mock<IByteEditTarget>().Object);
            var list = new ByteRecordListViewModel(new[] { first, second });
            var raised = new List<string?>();
            list.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            list.SelectedRecord = first;
            list.SelectedRecord = second;

            Assert.That(raised, Is.EqualTo(new[] { nameof(ByteRecordListViewModel.SelectedRecord) }));
        }

        [Test]
        public void CreateTargetMakesTheTargetOfThePickedRecordOrNothing()
        {
            var target = new Mock<IByteEditTarget>().Object;
            var list = new ByteRecordListViewModel(new[] { new ByteRecordViewModel("A", () => target) });

            Assert.That(list.CreateTarget(), Is.SameAs(target));

            list.SelectedRecord = null;
            Assert.That(list.CreateTarget(), Is.Null);
        }
    }
}
