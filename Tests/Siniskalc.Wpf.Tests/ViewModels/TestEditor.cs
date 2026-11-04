namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A navigation builder that makes one node for each record of <see cref="TestGame"/> and says what it was asked, with a structure
    /// key a test can change.
    /// </summary>
    internal sealed class TestNavigationBuilder : INavigationBuilder<TestGame>
    {
        public string Key { get; set; } = "one";

        public int Builds { get; private set; }

        public int Refreshes { get; private set; }

        public IReadOnlyDictionary<NavNodeKind, ChunkKind> ChunkKinds { get; } = new Dictionary<NavNodeKind, ChunkKind> { [NavNodeKind.Hero] = TestGame.RecordKind };

        public string StructureKey(TestGame game) => this.Key;

        public void Build(NavigationTree tree, TestGame game)
        {
            this.Builds++;
            var overview = new NavNodeViewModel("Overview", string.Empty, "#000000", NavNodeKind.Overview, onSelected: tree.OnSelected);
            tree.SetOverview(overview);
            for (var i = 0; i < 2; i++)
            {
                var node = new NavNodeViewModel($"Record {i}", string.Empty, "#000000", NavNodeKind.Hero, i, onSelected: tree.OnSelected);
                overview.Children.Add(node);
                tree.Register(node);
            }
        }

        public void RefreshMarks(NavigationTree tree, TestGame game)
        {
            this.Refreshes++;
        }
    }

    /// <summary>
    /// An editor of <see cref="TestGame"/> that notes how the base class used its hooks, and shows a string for each page.
    /// </summary>
    internal sealed class TestEditor : EditorViewModelBase<TestGame>
    {
        public TestEditor(ISavegameService<TestGame> service, ISaveRegionProvider regions, TestNavigationBuilder builder)
            : base(service, regions, builder)
        {
            this.Builder = builder;
        }

        public TestNavigationBuilder Builder { get; }

        public List<string> Hooks { get; } = [];

        public List<string> Showing { get; } = [];

        public List<NavNodeViewModel> SelectedNodes { get; } = [];

        public int HistoryChanges { get; set; }

        public new NavigationTree Navigation => base.Navigation;

        public object Overview { get; } = "overview page";

        /// <summary>Gives the page a node shows, when it is not the page of the same number (the Mines tab of a player, say).</summary>
        public Func<NavNodeViewModel, string>? PageOf { get; set; }

        public override AbstractFinderViewModel CreateArtifactCatalog() => throw new NotSupportedException();

        public override AbstractFinderViewModel CreateCreatureCatalog() => throw new NotSupportedException();

        protected override object? OverviewPage => this.Overview;

        public void Show(object? page) => this.SetCurrentPage(page is string text ? string.Intern(text) : page);

        protected override object? PrepareGame(TestGame game)
        {
            this.Hooks.Add("prepare");
            return "prepared " + game.Name;
        }

        protected override void CloseGame() => this.Hooks.Add("close");

        protected override void OpenGame(TestGame game, object? prepared) => this.Hooks.Add("open " + prepared);

        protected override NavNodeViewModel? NodeOf(object page) => page is string text && text.StartsWith("record ", StringComparison.Ordinal)
            ? this.Navigation.HeroNode(int.Parse(text[7..]))
            : this.Navigation.Overview;

        protected override void OnNodeSelected(NavNodeViewModel node)
        {
            this.SelectedNodes.Add(node);
            this.Show(node.Kind == NavNodeKind.Overview ? this.Overview : this.PageOf?.Invoke(node) ?? "record " + node.Index);
        }

        protected override void OnPageShowing(object? page) => this.Showing.Add(page?.ToString() ?? "none");

        protected override void OnHistoryChanged() => this.HistoryChanges++;
    }
}
