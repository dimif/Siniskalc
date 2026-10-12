using System.Windows;
using Siniskalc.Wpf.ViewModels;

namespace Siniskalc.Wpf.Tests.ViewModels
{
    /// <summary>
    /// Tests the behaviour every finder shares, with a small finder of fruit.
    /// </summary>
    [TestFixture]
    internal sealed class FinderViewModelTests
    {
        private sealed record Fruit(string Name, string Colour);

        private sealed class FruitFinder : FinderViewModel<Fruit>
        {
            public FruitFinder(Action<Fruit>? onChoose = null)
                : base(
                    [new("Apple", "Red"), new("Cherry", "Red"), new("Banana", "Yellow")],
                    nameof(Fruit.Colour),
                    string.Empty,
                    "fruits",
                    [new("Fruit", nameof(Fruit.Name), new GridLength(1, GridUnitType.Star))])
            {
                if (onChoose != null)
                {
                    this.SetAction("Eat", onChoose, f => f.Name != "Cherry");
                }

                this.StartFiltering();
            }

            protected override bool Matches(Fruit row) => this.TextMatches(row.Name, row.Colour);
        }

        private static string[] Names(AbstractFinderViewModel finder) => finder.Items.Cast<Fruit>().Select(f => f.Name).ToArray();

        [Test]
        public void EveryWordOfTheSearchTextMustMatchSomeField()
        {
            var finder = new FruitFinder();
            Assert.That(finder.Summary, Is.EqualTo("3 fruits"));
            Assert.That(finder.Items.Groups, Has.Count.EqualTo(2));

            finder.Filter = "red";
            Assert.That(Names(finder), Is.EquivalentTo(new[] { "Apple", "Cherry" }));
            Assert.That(finder.Summary, Is.EqualTo("showing 2 of 3 fruits"));

            finder.Filter = "RED ch";
            Assert.That(Names(finder), Is.EqualTo(new[] { "Cherry" }));

            finder.Filter = "red yellow";
            Assert.That(Names(finder), Is.Empty);

            finder.Filter = "  ";
            Assert.That(Names(finder), Has.Length.EqualTo(3));
        }

        [Test]
        public void ChangingTheSearchTextTellsTheViewTheSummaryChanged()
        {
            var finder = new FruitFinder();
            var changed = new List<string?>();
            finder.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            finder.Filter = "yellow";

            Assert.That(changed, Does.Contain(nameof(AbstractFinderViewModel.Summary)));
        }

        [Test]
        public void WithoutAnActionRowsCannotBeChosen()
        {
            var finder = new FruitFinder();

            Assert.That(finder.HasAction, Is.False);
            Assert.That(finder.ChooseCommand.CanExecute(finder.Items.GetItemAt(0)), Is.False);
        }

        [Test]
        public void TheActionRunsOnlyForRowsItAllows()
        {
            var eaten = new List<string>();
            var finder = new FruitFinder(f => eaten.Add(f.Name));
            var apple = finder.Items.Cast<Fruit>().Single(f => f.Name == "Apple");
            var cherry = finder.Items.Cast<Fruit>().Single(f => f.Name == "Cherry");

            Assert.That(finder.HasAction, Is.True);
            Assert.That(finder.ActionLabel, Is.EqualTo("Eat"));
            Assert.That(finder.Footer, Is.Not.Empty);

            finder.ChooseCommand.Execute(apple);
            finder.ChooseRow(cherry);
            finder.ChooseRow("not a fruit");

            Assert.That(eaten, Is.EqualTo(new[] { "Apple" }));
            Assert.That(finder.ChooseCommand.CanExecute(cherry), Is.False);
        }
    }
}
