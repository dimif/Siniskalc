namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Tests what every game editor does around the game it holds: loading and reloading, the history and its pending changes, the
    /// navigation tree and the page shown, and saving, over a service and a navigation builder that are fakes.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class EditorViewModelBaseTests
    {
        private Mock<ISavegameService<TestGame>> service = null!;
        private Mock<ISaveRegionProvider> regions = null!;
        private TestEditor editor = null!;
        private List<string> raised = null!;

        [SetUp]
        public void StartWithNoGame()
        {
            this.service = new Mock<ISavegameService<TestGame>>();
            this.service.Setup(s => s.LoadGameFromFile(It.IsAny<string>())).Returns<string>(f => new TestGame(f));
            this.service.Setup(s => s.LoadGameFromBytes(It.IsAny<byte[]>(), It.IsAny<string?>())).Returns<byte[], string?>((_, n) => new TestGame(null, n));
            this.regions = new Mock<ISaveRegionProvider>();
            this.regions.Setup(r => r.GetRegions(It.IsAny<SaveDocument>(), It.IsAny<byte[]>())).Returns([]);
            this.editor = new TestEditor(this.service.Object, this.regions.Object, new TestNavigationBuilder());
            this.raised = [];
            this.editor.PropertyChanged += (_, e) => this.raised.Add(e.PropertyName!);
        }

        [Test]
        public void WithNoGameNothingIsLoadedAndNothingCanBeUndoneOrSaved()
        {
            Assert.That((this.editor.IsGameLoaded, this.editor.CanUndo, this.editor.CanRedo, this.editor.HasPendingChanges), Is.EqualTo((false, false, false, false)));
            Assert.That((this.editor.CurrentGame, this.editor.Savegame, this.editor.CurrentPage), Is.EqualTo(((TestGame?)null, (ISavegame?)null, (object?)null)));
            Assert.That(() => this.editor.SaveGame(), Throws.InvalidOperationException);
            Assert.That(() => this.editor.SaveGame("x.GM1"), Throws.InvalidOperationException);
            Assert.That(() => this.editor.GetBytesToSave(), Throws.InvalidOperationException);
            Assert.That(() => this.editor.ReloadGame(), Throws.InvalidOperationException);
            Assert.That(() => this.editor.CreateSaveHexDump(), Throws.InvalidOperationException);
        }

        [Test]
        public void WithNoGameUndoRedoAndRevertDoNothing()
        {
            Assert.That(() => this.editor.Undo(), Throws.Nothing);
            Assert.That(() => this.editor.Redo(), Throws.Nothing);
            Assert.That(() => this.editor.RevertAll(), Throws.Nothing);
            Assert.That(() => this.editor.RevertChange(null), Throws.Nothing);
        }

        [Test]
        public void LoadingAFileAsksTheServiceAndHoldsTheGame()
        {
            this.editor.LoadGame("C:\\saves\\a.GM1");

            Assert.That(this.editor.CurrentGame!.FileName, Is.EqualTo("C:\\saves\\a.GM1"));
            Assert.That(this.editor.Savegame, Is.SameAs(this.editor.CurrentGame));
            Assert.That(this.editor.IsGameLoaded, Is.True);
        }

        [Test]
        public void LoadingBytesGivesTheServiceTheNameOfTheSource()
        {
            this.editor.LoadGameFromBytes([1, 2], "from memory");

            this.service.Verify(s => s.LoadGameFromBytes(new byte[] { 1, 2 }, "from memory"), Times.Once);
            Assert.That(this.editor.CurrentGame!.Name, Is.EqualTo("from memory"));
        }

        [Test]
        public void LoadingPreparesTheGameThenClosesTheOldOneThenOpensTheNewOneWithWhatWasPrepared()
        {
            this.editor.LoadGame("a.GM1");

            Assert.That(this.editor.Hooks, Is.EqualTo(new[] { "prepare", "close", "open prepared a.GM1" }));
        }

        [Test]
        public void LoadingTellsThatTheGameWasLoadedAndShowsTheOverview()
        {
            this.editor.LoadGame("a.GM1");

            Assert.That(this.raised, Is.SupersetOf(new[] { nameof(this.editor.CurrentGame), nameof(this.editor.IsGameLoaded), nameof(this.editor.Savegame), nameof(this.editor.CurrentPage) }));
            Assert.That(this.editor.CurrentPage, Is.SameAs(this.editor.Overview));
            Assert.That(this.editor.Navigation.Overview!.IsSelected, Is.True);
        }

        [Test]
        public void LoadingBuildsTheNavigationAndStartsWithNoPendingChange()
        {
            this.editor.LoadGame("a.GM1");

            Assert.That(this.editor.Builder.Builds, Is.EqualTo(1));
            Assert.That(this.editor.NavigationNodes.Select(n => n.Title), Is.EqualTo(new[] { "Overview" }));
            Assert.That(this.editor.Navigation.HeroNode(1)!.Title, Is.EqualTo("Record 1"));
            Assert.That(this.editor.PendingChanges, Is.Empty);
        }

        [Test]
        public void LoadingAnotherGameClosesTheFirstAndRebuildsTheNavigationAndStopsListeningToTheOldHistory()
        {
            this.editor.LoadGame("a.GM1");
            var first = this.editor.CurrentGame!;
            this.editor.LoadGame("b.GM1");
            this.editor.HistoryChanges = 0;

            first.Edit(0, 99);

            Assert.That(this.editor.Hooks.Count(h => h == "close"), Is.EqualTo(2));
            Assert.That(this.editor.Builder.Builds, Is.EqualTo(2));
            Assert.That(this.editor.HistoryChanges, Is.Zero);
            Assert.That(this.editor.PendingChanges, Is.Empty);
        }

        [Test]
        public void AnEditShowsAsAPendingChangeAndMarksTheNodeAndTellsTheHooks()
        {
            this.editor.LoadGame("a.GM1");
            this.editor.HistoryChanges = 0;

            this.editor.CurrentGame!.Edit(1, 99);

            Assert.That(this.editor.PendingChanges.Select(c => (c.OwnerName, c.Change.After)), Is.EqualTo(new[] { ("record 1", "99") }));
            Assert.That(this.editor.HasPendingChanges, Is.True);
            Assert.That(this.editor.Navigation.HeroNode(1)!.HasChanges, Is.True);
            Assert.That(this.editor.Navigation.HeroNode(0)!.HasChanges, Is.False);
            Assert.That(this.editor.Builder.Refreshes, Is.GreaterThan(0));
            Assert.That(this.editor.HistoryChanges, Is.EqualTo(1));
        }

        [Test]
        public void AnEditTellsThatUndoBecameAvailableAndUndoAndRedoGoBackAndForward()
        {
            this.editor.LoadGame("a.GM1");
            this.raised.Clear();

            this.editor.CurrentGame!.Edit(0, 99);

            Assert.That(this.raised, Is.SupersetOf(new[] { nameof(this.editor.CanUndo), nameof(this.editor.CanRedo), nameof(this.editor.HasPendingChanges) }));
            Assert.That((this.editor.CanUndo, this.editor.CanRedo), Is.EqualTo((true, false)));

            this.editor.Undo();
            Assert.That((this.editor.CanUndo, this.editor.CanRedo), Is.EqualTo((false, true)));
            Assert.That(this.editor.PendingChanges, Is.Empty);

            this.editor.Redo();
            Assert.That(this.editor.PendingChanges, Has.Count.EqualTo(1));
        }

        [Test]
        public void ChangesThatDidNotChangeAreNotTouchedWhenTheListIsRefreshed()
        {
            this.editor.LoadGame("a.GM1");
            this.editor.CurrentGame!.Edit(0, 99);
            var notifications = new List<NotifyCollectionChangedAction>();
            this.editor.PendingChanges.CollectionChanged += (_, e) => notifications.Add(e.Action);

            this.editor.CurrentGame.Edit(1, 77);

            Assert.That(this.editor.PendingChanges, Has.Count.EqualTo(2));
            Assert.That(notifications, Is.EqualTo(new[] { NotifyCollectionChangedAction.Add }), "only the new change is told");
        }

        [Test]
        public void RevertingAChangeRunsItsRevertOnTheHistory()
        {
            this.editor.LoadGame("a.GM1");
            SaveDocumentHistory? given = null;
            var change = new PendingChange("x", new Siniskalc.Core.Models.Change(new Siniskalc.Core.Models.ChangeArea("A"), "d", "1", "2"), TestGame.RecordKind, 0, h => given = h);

            this.editor.RevertChange(change);

            Assert.That(given, Is.SameAs(this.editor.CurrentGame!.History));
        }

        [Test]
        public void RevertingAllPutsTheOriginalDocumentBackOnlyWhenSomethingIsPending()
        {
            this.editor.LoadGame("a.GM1");
            var game = this.editor.CurrentGame!;

            this.editor.RevertAll();
            Assert.That(game.History.CanUndo, Is.False, "nothing pending, nothing done");

            game.Edit(0, 99);
            game.Edit(1, 77);
            this.editor.RevertAll();

            Assert.That(this.editor.PendingChanges, Is.Empty, "both edits are gone, not only the last");
            Assert.That(game.GetBytesToSave(), Is.EqualTo(new byte[] { 10, 20 }));
        }

        [Test]
        public void ANewStructureKeyRebuildsTheNavigationAndShowsTheSamePageAgain()
        {
            this.editor.LoadGame("a.GM1");
            this.editor.Show("record 1");
            this.editor.Navigation.HeroNode(1)!.IsSelected = false;

            this.editor.Builder.Key = "two";
            this.editor.CurrentGame!.Edit(0, 99);

            Assert.That(this.editor.Builder.Builds, Is.EqualTo(2));
            Assert.That(this.editor.Navigation.HeroNode(1)!.IsSelected, Is.True, "the page is revealed in the new tree");
        }

        [Test]
        public void TheSameStructureKeyLeavesTheNavigationAlone()
        {
            this.editor.LoadGame("a.GM1");

            this.editor.CurrentGame!.Edit(0, 99);

            Assert.That(this.editor.Builder.Builds, Is.EqualTo(1));
        }

        [Test]
        public void TheFilterOfTheNavigationIsKeptAndAppliedAgainWhenTheTreeIsRebuilt()
        {
            this.editor.LoadGame("a.GM1");
            this.editor.NavigationFilter = "record 1";
            this.editor.Builder.Key = "two";

            this.editor.CurrentGame!.Edit(0, 99);

            Assert.That(this.editor.Navigation.HeroNode(0)!.IsVisible, Is.False);
            Assert.That(this.editor.Navigation.HeroNode(1)!.IsVisible, Is.True);
        }

        [Test]
        public void SettingTheFilterTellsOnceAppliesItAndTreatsNullAsNone()
        {
            this.editor.LoadGame("a.GM1");
            this.raised.Clear();

            this.editor.NavigationFilter = "record 1";
            this.editor.NavigationFilter = "record 1";

            Assert.That(this.raised.Count(r => r == nameof(this.editor.NavigationFilter)), Is.EqualTo(1));
            Assert.That(this.editor.Navigation.HeroNode(0)!.IsVisible, Is.False);

            this.editor.NavigationFilter = null!;

            Assert.That(this.editor.NavigationFilter, Is.Empty);
            Assert.That(this.editor.Navigation.HeroNode(0)!.IsVisible, Is.True);
        }

        [Test]
        public void ShowingAPageTellsOnlyWhenItIsAnotherPageAndAsksTheHookFirst()
        {
            this.editor.LoadGame("a.GM1");
            this.editor.Showing.Clear();
            this.raised.Clear();

            this.editor.Show("record 0");
            this.editor.Show("record 0");

            Assert.That(this.editor.Showing, Is.EqualTo(new[] { "record 0" }));
            Assert.That(this.raised.Count(r => r == nameof(this.editor.CurrentPage)), Is.EqualTo(1));
            Assert.That(this.editor.CurrentPage, Is.EqualTo("record 0"));
        }

        [Test]
        public void ShowingAPageRevealsTheNodeOfThePage()
        {
            this.editor.LoadGame("a.GM1");

            this.editor.Show("record 1");

            Assert.That(this.editor.Navigation.HeroNode(1)!.IsSelected, Is.True);
        }

        [Test]
        public void ShowingNoPageRevealsNothing()
        {
            this.editor.LoadGame("a.GM1");

            this.editor.Show(null);

            Assert.That(this.editor.CurrentPage, Is.Null);
        }

        [Test]
        public void ShowOverviewShowsThePageOfTheOverview()
        {
            this.editor.LoadGame("a.GM1");
            this.editor.Show("record 0");

            this.editor.ShowOverview();

            Assert.That(this.editor.CurrentPage, Is.SameAs(this.editor.Overview));
        }

        [Test]
        public void PickingANodeTellsTheEditorAndTheNodePickedStaysTheOneSelected()
        {
            this.editor.LoadGame("a.GM1");
            var picked = this.editor.Navigation.HeroNode(0)!;
            this.editor.SelectedNodes.Clear();

            picked.IsSelected = true;

            Assert.That(this.editor.SelectedNodes, Is.EqualTo(new[] { picked }));
            Assert.That(this.editor.CurrentPage, Is.EqualTo("record 0"));
        }

        [Test]
        public void ThePickedNodeStaysTheSelectedOneEvenWhenThePageBelongsToAnotherNode()
        {
            this.editor.LoadGame("a.GM1");
            var picked = this.editor.Navigation.HeroNode(1)!;
            var owner = this.editor.Navigation.HeroNode(0)!;
            this.editor.PageOf = _ => "record 0";

            picked.IsSelected = true;

            Assert.That(this.editor.CurrentPage, Is.EqualTo("record 0"));
            Assert.That((picked.IsSelected, owner.IsSelected), Is.EqualTo((true, false)), "two nodes are not highlighted");
        }

        [Test]
        public void AfterThePickTheNodeOfThePageIsRevealedAgain()
        {
            this.editor.LoadGame("a.GM1");
            this.editor.Navigation.HeroNode(1)!.IsSelected = true;

            this.editor.Show("record 0");

            Assert.That(this.editor.Navigation.HeroNode(0)!.IsSelected, Is.True);
        }

        [Test]
        public void SavingWithoutAFileIsRefusedAndWithOneUsesIt()
        {
            this.editor.LoadGameFromBytes([1, 2], "memory");
            Assert.That(() => this.editor.SaveGame(), Throws.InvalidOperationException);

            this.editor.LoadGame("   ");
            Assert.That(() => this.editor.SaveGame(), Throws.InvalidOperationException, "a blank file name is no file");

            this.editor.LoadGame("a.GM1");
            this.raised.Clear();
            this.editor.SaveGame();

            this.service.Verify(s => s.SaveGame(this.editor.CurrentGame!, "a.GM1"), Times.Once);
            Assert.That(this.raised, Does.Contain(nameof(this.editor.Savegame)));
        }

        [Test]
        public void SavingToAPathGivesItToTheService()
        {
            this.editor.LoadGame("a.GM1");

            this.editor.SaveGame("C:\\saves\\b.GM1");

            this.service.Verify(s => s.SaveGame(this.editor.CurrentGame!, "C:\\saves\\b.GM1"), Times.Once);
        }

        [Test]
        public void ReloadingHoldsTheGameTheServiceGivesBackAndRebuildsEverything()
        {
            this.editor.LoadGame("a.GM1");
            var again = new TestGame("a.GM1");
            this.service.Setup(s => s.ReloadGame(this.editor.CurrentGame!)).Returns(again);

            this.editor.ReloadGame();

            Assert.That(this.editor.CurrentGame, Is.SameAs(again));
            Assert.That(this.editor.Builder.Builds, Is.EqualTo(2));
        }

        [Test]
        public void TheBytesToSaveAreThoseOfTheGame()
        {
            this.editor.LoadGame("a.GM1");
            this.editor.CurrentGame!.Edit(0, 99);

            Assert.That(this.editor.GetBytesToSave(), Is.EqualTo(new byte[] { 99, 20 }));
        }

        [Test]
        public void TheHexDumpIsTitledWithTheFileNameAndTheVersionOfTheGame()
        {
            this.editor.LoadGame("C:\\saves\\a.GM1");

            var dump = this.editor.CreateSaveHexDump();

            Assert.That(dump.Title, Is.EqualTo("a.GM1 (Test version)"));
            Assert.That(dump.Bytes, Is.EqualTo(new byte[] { 10, 20 }));
        }

        [Test]
        public void TheHexDumpOfAGameWithABlankNameIsCalledSavefile()
        {
            this.editor.LoadGameFromBytes([1, 2], " ");

            Assert.That(this.editor.CreateSaveHexDump().Title, Is.EqualTo("Savefile (Test version)"));
        }
    }
}
