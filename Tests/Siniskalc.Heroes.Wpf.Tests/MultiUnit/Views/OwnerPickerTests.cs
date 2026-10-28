namespace Siniskalc.Heroes.Wpf.Tests.MultiUnit.Views
{
    using System.ComponentModel;
    using System.Windows.Controls;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.Views;

    /// <summary>
    /// Tests the choice of who owns a hero or a castle.
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category(TestKinds.MultiUnit)]
    [Apartment(ApartmentState.STA)]
    internal sealed class OwnerPickerTests
    {
        [OneTimeSetUp]
        public void MakeTheApplication() => WpfTestApplication.Ensure();

        [Test]
        public void TheListOffersTheOwnersOfThePageAndPicksTheOneItHas()
        {
            var page = new FakeOwned { SelectedOwner = "Blue" };
            var picker = new OwnerPicker { DataContext = page };

            using var hosted = WpfTestApplication.Show(picker);

            var list = WpfTestApplication.Descendants<ComboBox>(picker).Single();
            Assert.That(list.Items.Cast<string>(), Is.EqualTo(new[] { "Neutral", "Blue", "Red" }));
            Assert.That(list.SelectedItem, Is.EqualTo("Blue"));
            Assert.That(WpfTestApplication.Descendants<TextBlock>(picker).Select(t => t.Text), Does.Contain("Owner"));
        }

        [Test]
        public void PickingAnOwnerSetsItOnThePage()
        {
            var page = new FakeOwned { SelectedOwner = "Blue" };
            var picker = new OwnerPicker { DataContext = page };
            using var hosted = WpfTestApplication.Show(picker);

            WpfTestApplication.Descendants<ComboBox>(picker).Single().SelectedItem = "Red";

            Assert.That(page.SelectedOwner, Is.EqualTo("Red"));
        }

        private sealed class FakeOwned : IOwnerChoice, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged
            {
                add { }
                remove { }
            }

            public IReadOnlyList<string> OwnerChoices { get; } = ["Neutral", "Blue", "Red"];

            public string SelectedOwner { get; set; } = "Neutral";
        }
    }
}
