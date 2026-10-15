namespace Siniskalc.Heroes.Wpf.Tests.Views
{
    using System.Windows.Controls;
    using Siniskalc.Heroes.Wpf.Views;

    /// <summary>
    /// Tests the card with a heading, a summary and content.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class HeadedCardTests
    {
        [OneTimeSetUp]
        public void MakeTheApplication() => WpfTestApplication.Ensure();

        [Test]
        public void TheCardShowsItsHeadingItsSummaryAndItsContentInACardOfTheWidthAsked()
        {
            var card = new HeadedCard { Heading = "Mage guild", Summary = "Level 2", Content = new TextBlock { Text = "Spells" }, CardWidth = 400 };

            using var hosted = WpfTestApplication.Show(card);

            Assert.That(WpfTestApplication.Descendants<TextBlock>(card).Select(t => t.Text), Is.SupersetOf(new[] { "Mage guild", "Level 2", "Spells" }));
            Assert.That(WpfTestApplication.Descendants<Border>(card).First().Width, Is.EqualTo(400));
        }
    }
}
