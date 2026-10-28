namespace Siniskalc.Heroes.Wpf.Tests.MultiUnit.Views
{
    using System.Windows.Controls;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Heroes.Wpf.Views;

    /// <summary>
    /// Tests what the Banishing card shows: a card whose template did not apply is empty and raises no binding error.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    [Apartment(ApartmentState.STA)]
    internal sealed class DaysLeftCardTests
    {
        [OneTimeSetUp]
        public void MakeTheApplication() => WpfTestApplication.Ensure();

        [Test]
        public void ThePlayerWhoOwnsNoTownGetsTheHeadingAndAButtonForEachDay()
        {
            var card = new DaysLeftCard { DataContext = new DaysLeftCardViewModel(() => 6, _ => { }, () => false) };

            using var hosted = WpfTestApplication.Show(card);

            var texts = WpfTestApplication.Descendants<TextBlock>(card).Where(t => t.IsVisible).Select(t => t.Text).ToList();
            Assert.That(texts, Is.SupersetOf(new[] { "Banishing", "Days left" }));
            var buttons = WpfTestApplication.Descendants<Button>(card).Where(b => b.IsVisible).ToList();
            Assert.That(buttons.Select(b => b.Content?.ToString()), Is.EqualTo(new[] { "7", "6", "5", "4", "3", "2", "1" }));
            Assert.That(buttons.Select(b => b.IsEnabled), Has.All.True);
        }

        [Test]
        public void ThePlayerWhoOwnsATownGetsNoCard()
        {
            var card = new DaysLeftCard { DataContext = new DaysLeftCardViewModel(() => null, _ => { }, () => false) };

            using var hosted = WpfTestApplication.Show(card);

            Assert.That(WpfTestApplication.Descendants<TextBlock>(card).Where(t => t.IsVisible), Is.Empty);
            Assert.That(WpfTestApplication.Descendants<Button>(card).Where(b => b.IsVisible), Is.Empty);
        }

        [Test]
        public void TheButtonsOfABanishedPlayerAreOff()
        {
            var card = new DaysLeftCard { DataContext = new DaysLeftCardViewModel(() => 0, _ => { }, () => true) };

            using var hosted = WpfTestApplication.Show(card);

            Assert.That(WpfTestApplication.Descendants<Button>(card).Select(b => b.IsEnabled), Has.All.False);
        }
    }
}
