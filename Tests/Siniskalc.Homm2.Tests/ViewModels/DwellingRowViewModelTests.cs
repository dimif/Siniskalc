namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the row of one dwelling of a castle of HoMM2.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class DwellingRowViewModelTests
    {
        [Test]
        public void ADwellingRowShowsWhetherItIsBuiltAndItsGrowthFromTheFunctionsItWasGiven()
        {
            var built = true;
            var growth = 7;
            var row = new DwellingRowViewModel(
                new ChoiceFieldViewModel("Built", ["Yes", "No"], () => built ? "Yes" : "No", _ => { }),
                new AmountRowViewModel("Waiting", "#000000", 0, 100, 1, () => 5, () => 5, _ => { }),
                () => built,
                () => growth);

            Assert.That(row.CanStock, Is.True);
            Assert.That(row.WeeklyGrowthText, Is.EqualTo(Siniskalc.Heroes.Wpf.ViewModels.WeeklyGrowthText.Of(7)));

            built = false;
            growth = 3;
            Assert.That(row.CanStock, Is.False);
            Assert.That(row.WeeklyGrowthText, Is.EqualTo(Siniskalc.Heroes.Wpf.ViewModels.WeeklyGrowthText.Of(3)));
        }

        [Test]
        public void RefreshingADwellingRowRefreshesItsFieldsAndTellsTheBindings()
        {
            var built = new ChoiceFieldViewModel("Built", ["Yes", "No"], () => "Yes", _ => { });
            var waiting = new AmountRowViewModel("Waiting", "#000000", 0, 100, 1, () => 5, () => 5, _ => { });
            var row = new DwellingRowViewModel(built, waiting, () => true, () => 1);
            var raised = new List<string>();
            built.PropertyChanged += (_, _) => raised.Add("built");
            waiting.PropertyChanged += (_, _) => raised.Add("waiting");
            row.PropertyChanged += (_, _) => raised.Add("row");

            row.Refresh();

            Assert.That(raised, Is.SupersetOf(new[] { "built", "waiting", "row" }));
            Assert.That((row.Built, row.Waiting), Is.EqualTo((built, waiting)));
        }
    }
}
