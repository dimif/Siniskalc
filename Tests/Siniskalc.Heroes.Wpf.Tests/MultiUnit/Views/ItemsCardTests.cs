namespace Siniskalc.Heroes.Wpf.Tests.MultiUnit.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using Siniskalc.Heroes.Wpf.Views;

    /// <summary>
    /// Tests the card with a heading and a list.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    [Apartment(ApartmentState.STA)]
    internal sealed class ItemsCardTests
    {
        [OneTimeSetUp]
        public void MakeTheApplication() => WpfTestApplication.Ensure();

        [Test]
        public void TheCardShowsItsHeadingAndTheItemsOfItsList()
        {
            var card = new ItemsCard { Heading = "Heroes", ItemsSource = new[] { "Ariel", "Maximus" }, EmptyText = "This player has no heroes" };

            using var hosted = WpfTestApplication.Show(card);

            var texts = WpfTestApplication.Descendants<TextBlock>(card).Select(t => t.Text).ToList();
            Assert.That(texts, Does.Contain("Heroes"));
            Assert.That(texts, Does.Contain("Ariel").And.Contain("Maximus"));
        }

        [Test]
        public void TheEmptyNoteShowsOnlyWhileTheListIsEmpty()
        {
            var card = new ItemsCard { Heading = "Towns", EmptyText = "This player has no towns", HasItems = true };
            using var hosted = WpfTestApplication.Show(card);
            var note = WpfTestApplication.Descendants<TextBlock>(card).Single(t => t.Text == "This player has no towns");
            Assert.That(note.Visibility, Is.EqualTo(Visibility.Collapsed));

            card.HasItems = false;

            Assert.That(note.Visibility, Is.EqualTo(Visibility.Visible));
        }
    }
}
