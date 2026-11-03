namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the heading of a sortable column: its label with the arrow of the direction and its command.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SortColumnViewModelTests
    {
        [Test]
        public void ANewHeadingShowsItsNameAndHoldsItsCommand()
        {
            var command = new DelegateCommand(_ => { });

            var column = new SortColumnViewModel("Name", command);

            Assert.That((column.Name, column.Label), Is.EqualTo(("Name", "Name")));
            Assert.That(column.SortCommand, Is.SameAs(command));
        }

        [Test]
        public void ShowingADirectionPutsTheArrowAfterTheName()
        {
            var column = new SortColumnViewModel("Name", new DelegateCommand(_ => { }));

            column.ShowDirection("▲");
            Assert.That(column.Label, Is.EqualTo("Name ▲"));

            column.ShowDirection("▼");
            Assert.That(column.Label, Is.EqualTo("Name ▼"));
        }

        [Test]
        public void ShowingNoDirectionTakesTheArrowAway()
        {
            var column = new SortColumnViewModel("Name", new DelegateCommand(_ => { }));
            column.ShowDirection("▲");

            column.ShowDirection(string.Empty);

            Assert.That(column.Label, Is.EqualTo("Name"));
        }

        [Test]
        public void ShowingADirectionTellsThatTheLabelChanged()
        {
            var column = new SortColumnViewModel("Name", new DelegateCommand(_ => { }));
            var raised = new List<string?>();
            column.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            column.ShowDirection("▲");

            Assert.That(raised, Is.EqualTo(new[] { nameof(SortColumnViewModel.Label) }));
        }
    }
}
