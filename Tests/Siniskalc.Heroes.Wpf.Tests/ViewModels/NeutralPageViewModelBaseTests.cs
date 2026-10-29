namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests what every game's page of the things that belong to nobody does: lists the heroes and castles it was given, words the
    /// summary and follows the history of the savefile. The game's part is a fake.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class NeutralPageViewModelBaseTests
    {
        private sealed class Page : NeutralPageViewModelBase
        {
            public Page(SaveDocumentHistory history)
                : base(history, "#808080")
            {
            }

            public Page(SaveDocumentHistory history, string name)
                : base(history, "#808080", name)
            {
            }

            public int HeroCount { get; set; }

            public int CastleCount { get; set; }

            public string Objects { get; set; } = string.Empty;

            public int ObjectRefreshes { get; private set; }

            public int Loads { get; private set; }

            protected override string ObjectsSummary => this.Objects;

            public static string Words(int count, string one, string many) => Count(count, one, many);

            protected override IReadOnlyList<ILink> LoadHeroes()
            {
                this.Loads++;
                return Links(this.HeroCount, "Hero");
            }

            protected override IReadOnlyList<ILink> LoadCastles() => Links(this.CastleCount, "Castle");

            protected override void RefreshObjects() => this.ObjectRefreshes++;

            private static List<ILink> Links(int count, string name) =>
                [.. Enumerable.Range(1, count).Select(i => new LinkViewModel($"{name} {i}", string.Empty, new DelegateCommand(_ => { })))];
        }

        private static SaveDocumentHistory History() => new(new SaveDocument(new byte[2], new Dictionary<ChunkKind, IChunkCollection>(), new SavefileBytesPatcher()));

        [Test]
        public void ThePageIsCalledNeutralUnlessAnotherNameIsGivenAndHasItsColour()
        {
            var plain = new Page(History());
            var named = new Page(History(), "Unowned");

            Assert.That((plain.Name, plain.Title, plain.ColorHex), Is.EqualTo(("Neutral", "Neutral", "#808080")));
            Assert.That(named.Name, Is.EqualTo("Unowned"));
        }

        [Test]
        public void ThereIsNothingToResetOnAPageOfThingsThatBelongToNobody()
        {
            Assert.That(new Page(History()).ResetCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void AFreshPageListsNothingUntilItIsRefreshed()
        {
            var page = new Page(History()) { HeroCount = 2, CastleCount = 1 };

            Assert.That((page.HasHeroes, page.HasCastles, page.Summary), Is.EqualTo((false, false, string.Empty)));
        }

        [Test]
        public void RefreshLoadsTheHeroesAndTheCastlesAndLetsTheGameRefreshItsObjects()
        {
            var page = new Page(History()) { HeroCount = 2, CastleCount = 1 };

            page.Refresh();

            Assert.That(page.Heroes.Select(h => h.Name), Is.EqualTo(new[] { "Hero 1", "Hero 2" }));
            Assert.That(page.Castles.Select(c => c.Name), Is.EqualTo(new[] { "Castle 1" }));
            Assert.That((page.HasHeroes, page.HasCastles, page.ObjectRefreshes), Is.EqualTo((true, true, 1)));
        }

        [Test]
        public void OneHeroOrOneCastleIsEnoughToHaveThem()
        {
            var page = new Page(History()) { HeroCount = 1, CastleCount = 1 };

            page.Refresh();

            Assert.That((page.HasHeroes, page.HasCastles), Is.EqualTo((true, true)));
        }

        [Test]
        public void RefreshTellsEveryBinding()
        {
            var page = new Page(History());
            var raised = new List<string?>();
            page.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            page.Refresh();

            Assert.That(raised, Does.Contain(string.Empty));
        }

        [Test]
        public void TheSummaryCountsTheHeroesTheTownsAndTheObjectsInOneLine()
        {
            var page = new Page(History()) { HeroCount = 3, CastleCount = 2, Objects = "5 mines" };

            page.Refresh();

            Assert.That(page.Summary, Is.EqualTo("3 heroes · 2 towns · 5 mines"));
        }

        [Test]
        public void TheSummaryLeavesOutWhatThereIsNoneOfAndSaysOneInTheSingular()
        {
            var page = new Page(History()) { HeroCount = 1, CastleCount = 0, Objects = string.Empty };

            page.Refresh();

            Assert.That(page.Summary, Is.EqualTo("1 hero"));
        }

        [Test]
        public void ThePhraseForACountIsEmptyForNoneAndSingularForOne()
        {
            Assert.That(Page.Words(0, "hero", "heroes"), Is.Empty);
            Assert.That(Page.Words(1, "hero", "heroes"), Is.EqualTo("1 hero"));
            Assert.That(Page.Words(2, "hero", "heroes"), Is.EqualTo("2 heroes"));
        }

        [Test]
        public void ThePageRefreshesWhenTheHistoryOfTheSavefileChanges()
        {
            var history = History();
            var page = new Page(history) { HeroCount = 1 };
            page.Refresh();
            page.HeroCount = 2;

            history.Apply(history.Current);

            Assert.That(page.Heroes, Has.Count.EqualTo(2));
        }

        [Test]
        public void ADisposedPageNoLongerFollowsTheHistory()
        {
            var history = History();
            var page = new Page(history) { HeroCount = 1 };
            page.Refresh();
            page.HeroCount = 2;

            page.Dispose();
            history.Apply(history.Current);

            Assert.That(page.Heroes, Has.Count.EqualTo(1));
            Assert.That(page.Loads, Is.EqualTo(1));
        }

        [Test]
        public void ThePageIsATabbedPageSoItKeepsTheTabThatIsShown()
        {
            var page = new Page(History());

            page.SelectedTab = 2;

            Assert.That(page.SelectedTab, Is.EqualTo(2));
        }
    }
}
