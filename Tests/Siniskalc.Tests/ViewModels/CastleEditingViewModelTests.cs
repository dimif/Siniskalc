using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Siniskalc;
using Siniskalc.Data;
using Siniskalc.Models;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;
using Siniskalc.Views;
using Siniskalc.Wpf.Views;

namespace Siniskalc.Tests.ViewModels
{
    /// <summary>
    /// Edits a castle through its page and view model on the bulwark_24 savegame ("Still Water", Rampart, red, and
    /// "Jotunngard", Bulwark, red), and opens the real window to catch XAML that does not load or bindings that do not exist.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class CastleEditingViewModelTests
    {
        private MainViewModel main = null!;
        private string path = null!;

        [SetUp]
        public void LoadGame()
        {
            this.path = Siniskalc.Tests.Data.CastleSamples.Folder("Bulwark").Single(p => p.EndsWith("bulwark_24_nothing__fair.GM1"));
            this.main = TestServices.CreateMainViewModel();
            this.main.LoadGameFromBytes(TestServices.SavefileCodec.Read(this.path), this.path);
        }

        private CastleViewModel Show(string name)
        {
            var index = this.main.LoadedGame().History.Current.Castles().FindByName(name);
            this.main.ShowCastle(index);
            return this.main.CurrentPageOf<CastleViewModel>();
        }

        private static BuildingRowViewModel Row(CastleViewModel castle, string name) =>
            castle.BuildingGroups.SelectMany(g => g.Rows).First(r => r.Name == name);

        [Test]
        public void TickingABuildingBuildsItAndTheChangeIsPending()
        {
            var castle = this.Show("Still Water");
            var row = Row(castle, "Upg. Centaur Stables");
            Assert.That(row.IsChecked, Is.False);
            Assert.That(row.CanToggle, Is.True);

            row.IsChecked = true;

            Assert.That(Row(castle, "Upg. Centaur Stables").IsBuilt, Is.True);
            Assert.That(this.main.PendingChanges.Select(c => c.Path), Is.EqualTo(new[] { "Still Water › Buildings" }));
            Assert.That(this.main.CanUndo, Is.True);
            Assert.That(castle.HasPendingChanges, Is.True);

            this.main.RevertAll();
            Assert.That(Row(castle, "Upg. Centaur Stables").IsBuilt, Is.False);
            Assert.That(castle.HasPendingChanges, Is.False);
        }

        [Test]
        public void ABuildingThatCannotBeBuiltSaysWhy()
        {
            var castle = this.Show("Still Water");

            // The game sets the bit of the upgraded growth building by itself; it is not a building and is not listed.
            Assert.That(this.Show("Jotunngard").BuildingGroups.SelectMany(g => g.Rows).Select(r => r.Name), Does.Not.Contain("Upg. Hay Bins"));

            var fort = Row(castle, "Fort");
            Assert.That(fort.CanToggle, Is.False);
            Assert.That(fort.ToggleToolTip, Does.StartWith("Needed by Citadel"));
            fort.IsChecked = false;
            Assert.That(Row(castle, "Fort").IsBuilt, Is.True);
        }

        [Test]
        public void ARefusedEditShowsANoticeAndChangesNothing()
        {
            var castle = this.Show("Still Water");

            castle.SelectedMageGuildLevel = 9;

            Assert.That(castle.HasNotice, Is.True);
            Assert.That(castle.Notice, Does.Contain("level 5"));
            Assert.That(this.main.PendingChanges, Is.Empty);

            castle.SelectedMageGuildLevel = 2;
            Assert.That(castle.HasNotice, Is.False);
        }

        [Test]
        public void TheMageGuildLevelCanBeRaisedAndLowered()
        {
            var castle = this.Show("Still Water");
            Assert.That(castle.MageGuildLevelChoices, Is.EqualTo(new[] { 0, 1, 2, 3, 4, 5 }));
            Assert.That(castle.SelectedMageGuildLevel, Is.EqualTo(4));

            castle.SelectedMageGuildLevel = 5;
            Assert.That(castle.MageGuildSummary, Is.EqualTo("Level 5 of 5"));
            Assert.That(castle.MageGuildLevels.Select(l => l.SpellsText), Is.EqualTo(new[] { "5 spells", "4 spells", "3 spells", "2 spells", "1 spells" }));

            // The dragon cliffs of a Rampart castle need the second level, but that does not stop the guild from going lower.
            castle.SelectedMageGuildLevel = 1;
            Assert.That(castle.Notice, Is.Empty);
            Assert.That(castle.SelectedMageGuildLevel, Is.EqualTo(1));

            castle.SelectedMageGuildLevel = 2;
            Assert.That(castle.Notice, Is.Empty);
            Assert.That(castle.MageGuildLevels.Select(l => l.IsBuilt), Is.EqualTo(new[] { true, true, false, false, false }));
            Assert.That(this.Show("Jotunngard").MageGuildLevelChoices, Is.EqualTo(new[] { 0, 1, 2, 3, 4 }));
        }

        [Test]
        public void TheDwellingCountIsEditedFromTheRow()
        {
            var castle = this.Show("Still Water");
            var row = castle.Dwellings.Single(d => d.Level == 3);
            Assert.That(row.CanEdit, Is.True);

            row.Count = 55;

            Assert.That(castle.Dwellings.Single(d => d.Level == 3).Count, Is.EqualTo(55));
            Assert.That(castle.Dwellings.Single(d => d.Level == 3).Creatures, Is.EqualTo("55"));
            Assert.That(this.main.PendingChanges.Single().Change.Area, Is.EqualTo(ChangeAreas.Dwellings));

            castle.Dwellings.Single(d => d.Level == 3).Count = Constants.Castle.MaxDwellingCreatures + 1;
            Assert.That(castle.Dwellings.Single(d => d.Level == 3).Count, Is.EqualTo(Constants.Castle.MaxDwellingCreatures));
        }

        [Test]
        public void TheOwnerIsPickedFromTheListAndTheCastleMovesInTheTree()
        {
            var castle = this.Show("Still Water");
            Assert.That(castle.OwnerChoices, Is.EqualTo(new[] { "Neutral", "Red", "Blue", "Tan", "Green", "Orange", "Purple", "Teal", "Pink" }));
            Assert.That(castle.SelectedOwner, Is.EqualTo("Red"));

            castle.SelectedOwner = "Blue";

            Assert.That(castle.OwnerText, Is.EqualTo("Blue"));
            Assert.That(this.main.PendingChanges.Single().Change.After, Is.EqualTo("Blue"));
            var blue = this.main.NavigationNodes.Descendants().Single(n => n.Title == "Blue" && n.Kind != NavNodeKind.Castle);
            Assert.That(blue.Descendants().Where(n => n.Kind == NavNodeKind.Castle).Select(n => n.Title), Is.EqualTo(new[] { "Still Water" }));
            Assert.That(this.main.NavigationNodes.Descendants().Single(n => n.Kind == NavNodeKind.Castle && n.Title == "Still Water").HasChanges, Is.True);

            castle.SelectedOwner = "Neutral";
            Assert.That(castle.OwnerText, Is.EqualTo("Neutral"));

            this.main.RevertAll();
            Assert.That(castle.SelectedOwner, Is.EqualTo("Red"));
        }

        [Test]
        public void TheGarrisonIsEditedThroughTheSharedArmyEditor()
        {
            var castle = this.Show("Still Water");

            castle.GarrisonEditor.CreatureSlots[2].SelectedCreature = "Pikeman";
            castle.GarrisonEditor.CreatureSlots[2].CreatureAmount = 40;

            Assert.That(castle.Garrison[2].CreatureName, Is.EqualTo("Pikeman"));
            Assert.That(castle.Garrison[2].CountText, Is.EqualTo("40"));
            Assert.That(this.main.PendingChanges.Single().Change.Description, Is.EqualTo("Slot 3"));
            Assert.That(castle.Garrisoned, Is.EqualTo("1 of 7 slots in use"));

            castle.GarrisonEditor.CreatureSlots[2].ClearCommand.Execute(null);
            Assert.That(this.main.PendingChanges, Is.Empty);
        }

        [Test]
        public void BuiltThisTurnIsAFreeFlagThatBuildingNeverChanges()
        {
            var castle = this.Show("Still Water");
            Assert.That(castle.BuiltThisTurn, Is.False);

            Row(castle, "Upg. Centaur Stables").IsChecked = true;
            Assert.That(castle.BuiltThisTurn, Is.False);

            castle.BuiltThisTurn = true;
            Assert.That(castle.BuiltThisTurn, Is.True);
            Assert.That(this.main.PendingChanges.Any(c => c.Change.Description == "Built this turn"), Is.True);

            Row(castle, "Upg. Centaur Stables").IsChecked = false;
            Assert.That(castle.BuiltThisTurn, Is.True);

            this.main.Undo();
            Assert.That(castle.BuiltThisTurn, Is.True);
            this.main.Undo();
            Assert.That(castle.BuiltThisTurn, Is.False);
            Assert.That(this.main.PendingChanges.Any(c => c.Change.Description == "Built this turn"), Is.False);
        }

        [Test]
        public void ResetTownPutsEverythingBack()
        {
            var castle = this.Show("Still Water");
            castle.SelectedOwner = "Pink";
            castle.SelectedMageGuildLevel = 1;
            castle.BuiltThisTurn = true;

            Assert.That(castle.ResetCommand.CanExecute(null), Is.True);
            castle.ResetCommand.Execute(null);

            Assert.That(castle.HasPendingChanges, Is.False);
            Assert.That(this.main.PendingChanges, Is.Empty);
            Assert.That(castle.ResetCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void TheCastlePageLoadsWithoutBindingErrorsAndEditsFlowThroughIt()
        {
            var errors = new BindingErrorListener();
            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(errors);
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

            try
            {
                EnsureApplication();
                var window = TestServices.CreateMainWindow(this.main);
                window.Show();

                try
                {
                    var frame = (Frame)window.FindName("heroFrame");
                    Pump();

                    foreach (var node in this.main.NavigationNodes.Descendants().Where(c => c.Kind == NavNodeKind.Castle && (c.Title == "Still Water" || c.Title == "Jotunngard")).ToList())
                    {
                        node.IsSelected = true;
                        Pump();
                        var page = (CastlePage)frame.Content;
                        var tabs = (ListBox)page.FindName("tabs");
                        foreach (var tab in tabs.Items.OfType<NavButton>())
                        {
                            tabs.SelectedItem = tab;
                            Pump();
                        }

                        tabs.SelectedIndex = 0;
                        var castle = this.main.CurrentPageOf<CastleViewModel>();
                        Row(castle, castle.Name == "Still Water" ? "Dragon Cliffs" : "Frosthome").IsChecked = false;
                        castle.Dwellings.First(d => d.CanEdit).Count = 7;
                        castle.SelectedMageGuildLevel = 3;
                        castle.GarrisonEditor.CreatureSlots[0].SelectedCreature = "Angel";
                        castle.BuiltThisTurn = true;
                        castle.SelectedOwner = "Teal";
                        Pump();
                        foreach (var tab in tabs.Items.OfType<NavButton>())
                        {
                            tabs.SelectedItem = tab;
                            Pump();
                        }
                    }

                    this.main.IsPendingChangesOpen = true;
                    Pump();
                    Assert.That(this.main.PendingChanges, Is.Not.Empty);
                    this.main.RevertAll();
                    Pump();
                    Assert.That(this.main.PendingChanges, Is.Empty);
                    this.main.ShowOverview();
                    Pump();
                }
                finally
                {
                    window.Close();
                }
            }
            finally
            {
                PresentationTraceSources.DataBindingSource.Listeners.Remove(errors);
            }

            Assert.That(errors.Messages, Is.Empty, string.Join(Environment.NewLine, errors.Messages));
        }

        private static void EnsureApplication()
        {
            if (Application.Current == null)
            {
                var app = new Siniskalc.App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                app.InitializeComponent();
            }
        }

        private static void Pump()
        {
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
        }

        private sealed class BindingErrorListener : TraceListener
        {
            public List<string> Messages { get; } = [];

            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message)
            {
                this.Messages.Add(message ?? string.Empty);
            }
        }
    }
}
