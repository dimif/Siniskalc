namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// What the editor of any game has in common: the open savegame and its history, loading, saving, undo, the pending changes, the
    /// navigation tree and the page that is shown. A game's editor says how to read its savefile into pages (<see cref="PrepareGame"/>,
    /// <see cref="OpenGame"/>, <see cref="CloseGame"/>), which node a page belongs to and what happens when a node is picked.
    /// The application shell works with it through <see cref="IGameEditor"/>, and its view through <see cref="INavigationEditor"/>.
    /// </summary>
    /// <typeparam name="TGame">What the game calls an open savefile.</typeparam>
    public abstract class EditorViewModelBase<TGame> : AbstractViewModelBase, IObservableGameEditor, INavigationEditor
        where TGame : class, ISavegame
    {
        private readonly ISavegameService<TGame> gameService;
        private readonly ISaveRegionProvider saveRegions;
        private readonly INavigationBuilder<TGame> navigationBuilder;
        private readonly NavigationTree navigation;
        private TGame? currentGame;
        private object? currentPage;
        private NavNodeViewModel? pickedNode;
        private string navigationFilter = string.Empty;
        private string structureKey = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorViewModelBase"/> class.
        /// </summary>
        /// <param name="gameService">The game service.</param>
        /// <param name="saveRegions">The save regions.</param>
        /// <param name="navigationBuilder">The navigation builder.</param>
        protected EditorViewModelBase(ISavegameService<TGame> gameService, ISaveRegionProvider saveRegions, INavigationBuilder<TGame> navigationBuilder)
        {
            this.gameService = gameService;
            this.saveRegions = saveRegions;
            this.navigationBuilder = navigationBuilder;
            this.navigation = new NavigationTree(this.OnNodePicked, navigationBuilder.ChunkKinds);
        }

        /// <summary>
        /// Gets the loaded game, or <c>null</c> when none is loaded.
        /// </summary>
        public TGame? CurrentGame => this.currentGame;

        /// <inheritdoc/>
        public ISavegame? Savegame => this.currentGame;

        /// <inheritdoc/>
        public object? CurrentPage => this.currentPage;

        /// <inheritdoc/>
        public ObservableCollection<NavNodeViewModel> NavigationNodes => this.navigation.Nodes;

        /// <inheritdoc/>
        public string NavigationFilter
        {
            get => this.navigationFilter;
            set
            {
                value ??= string.Empty;
                if (this.navigationFilter == value)
                {
                    return;
                }

                this.navigationFilter = value;
                this.RaisePropertyChanged(nameof(this.NavigationFilter));
                this.navigation.ApplyFilter(value);
            }
        }

        /// <inheritdoc/>
        public bool IsGameLoaded => this.currentGame != null;

        /// <inheritdoc/>
        public bool CanUndo => this.currentGame?.History.CanUndo ?? false;

        /// <inheritdoc/>
        public bool CanRedo => this.currentGame?.History.CanRedo ?? false;

        /// <inheritdoc/>
        public ObservableCollection<PendingChange> PendingChanges { get; } = [];

        /// <inheritdoc/>
        public bool HasPendingChanges => this.PendingChanges.Count > 0;

        /// <summary>
        /// Gets the page that shows the whole savegame, which is what opens first.
        /// </summary>
        protected abstract object? OverviewPage { get; }

        /// <summary>
        /// Gets the navigation tree, for what a game's editor reveals in it.
        /// </summary>
        protected NavigationTree Navigation => this.navigation;

        /// <inheritdoc/>
        public void LoadGame(string filePath) => this.SetGame(this.gameService.LoadGameFromFile(filePath));

        /// <inheritdoc/>
        public void LoadGameFromBytes(byte[] bytes, string? sourceName = null) => this.SetGame(this.gameService.LoadGameFromBytes(bytes, sourceName));

        /// <inheritdoc/>
        public virtual void ReloadGame()
        {
            this.SetGame(this.gameService.ReloadGame(this.Loaded()));
        }

        /// <inheritdoc/>
        public void SaveGame()
        {
            var file = this.currentGame?.FileName;
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new InvalidOperationException("The game has no file to save to");
            }

            this.SaveGame(file);
        }

        /// <inheritdoc/>
        public void SaveGame(string filePath)
        {
            this.gameService.SaveGame(this.Loaded(), filePath);
            this.RaisePropertyChanged(nameof(this.Savegame));
        }

        /// <inheritdoc/>
        public byte[] GetBytesToSave() => this.Loaded().GetBytesToSave();

        /// <inheritdoc/>
        public SaveHexDump CreateSaveHexDump()
        {
            var game = this.Loaded();
            var name = string.IsNullOrWhiteSpace(game.Name) ? "Savefile" : System.IO.Path.GetFileName(game.Name);
            return SaveHexDump.Create($"{name} ({game.VersionName})", game.History, this.saveRegions);
        }

        /// <inheritdoc/>
        public void Undo() => this.currentGame?.History.Undo();

        /// <inheritdoc/>
        public void Redo() => this.currentGame?.History.Redo();

        /// <inheritdoc/>
        public void RevertChange(PendingChange? change)
        {
            if (this.currentGame != null)
            {
                change?.Revert(this.currentGame.History);
            }
        }

        /// <inheritdoc/>
        public void RevertAll()
        {
            if (this.currentGame != null && this.HasPendingChanges)
            {
                this.currentGame.History.ApplyAsIs(this.currentGame.History.Original);
            }
        }

        /// <summary>
        /// Shows the overview of the savegame.
        /// </summary>
        public void ShowOverview() => this.SetCurrentPage(this.OverviewPage);

        /// <summary>
        /// Gets the open game, or throws when none is loaded.
        /// </summary>
        /// <returns>The open game.</returns>
        /// <exception cref="InvalidOperationException">No game is loaded.</exception>
        protected TGame Loaded() => this.currentGame ?? throw new InvalidOperationException("No game is currently loaded");

        /// <summary>
        /// Builds what reads <paramref name="game"/> (and so can fail on a savefile that is odd), before anything of the open game is
        /// let go, so a game that cannot be shown leaves the one that is open as it was. What it returns goes to <see cref="OpenGame"/>.
        /// </summary>
        /// <returns>What <see cref="OpenGame"/> needs, or <see langword="null"/> for nothing.</returns>
        /// <param name="game">The game being opened.</param>
        protected abstract object? PrepareGame(TGame game);

        /// <summary>
        /// Lets go of the pages of the open game.
        /// </summary>
        protected abstract void CloseGame();

        /// <summary>
        /// Takes <paramref name="game"/> (now the current game) and what <see cref="PrepareGame"/> made of it into use.
        /// </summary>
        /// <param name="prepared">What <see cref="PrepareGame"/> returned.</param>
        /// <param name="game">The game being opened.</param>
        protected abstract void OpenGame(TGame game, object? prepared);

        /// <summary>
        /// Gets the node of the navigation tree that <paramref name="page"/> belongs to, or <c>null</c> when it has none.
        /// </summary>
        /// <returns>The node, or <see langword="null"/>.</returns>
        /// <param name="page">The view model of the page.</param>
        protected abstract NavNodeViewModel? NodeOf(object page);

        /// <summary>
        /// Shows what <paramref name="node"/>, which the user picked, stands for.
        /// </summary>
        /// <param name="node">The node the user picked.</param>
        protected abstract void OnNodeSelected(NavNodeViewModel node);

        /// <summary>
        /// Called when <paramref name="page"/> is about to be shown, in place of the one that is.
        /// </summary>
        /// <param name="page">The view model of the page, or <see langword="null"/> for none.</param>
        protected virtual void OnPageShowing(object? page)
        {
        }

        /// <summary>
        /// Called after an edit, an undo or a redo, when the pending changes and the tree are up to date.
        /// </summary>
        protected virtual void OnHistoryChanged()
        {
        }

        /// <summary>
        /// Makes <paramref name="page"/> the page the editor shows.
        /// </summary>
        /// <param name="page">The view model of the page, or <see langword="null"/> for none.</param>
        protected void SetCurrentPage(object? page)
        {
            if (!ReferenceEquals(this.currentPage, page))
            {
                this.RefreshOverview(page);
                this.OnPageShowing(page);
                this.currentPage = page;
                this.RaisePropertyChanged(nameof(this.CurrentPage));
            }

            if (page != null)
            {
                this.navigation.Reveal(this.pickedNode ?? this.NodeOf(page));
            }
        }

        // The node the user picked stays the selected one while its page is shown, even when the page belongs to another node as well (the
        // Mines tab of a player for one of its mines); selecting that node too would leave two nodes highlighted.
        private void OnNodePicked(NavNodeViewModel node)
        {
            this.pickedNode = node;
            try
            {
                this.OnNodeSelected(node);
            }
            finally
            {
                this.pickedNode = null;
            }
        }

        private void SetGame(TGame game)
        {
            var prepared = this.PrepareGame(game);
            var newKey = this.navigationBuilder.StructureKey(game);

            this.CloseGame();

            if (this.currentGame != null)
            {
                this.currentGame.History.Changed -= this.OnHistoryChangedCore;
            }

            this.currentPage = null;
            this.currentGame = game;
            this.structureKey = newKey;
            this.currentGame.History.Changed += this.OnHistoryChangedCore;
            this.OpenGame(game, prepared);
            this.BuildNavigation(game);
            this.PendingChanges.Clear();
            this.RaisePropertyChanged(nameof(this.CurrentGame));
            this.RaisePropertyChanged(nameof(this.IsGameLoaded));
            this.RaisePropertyChanged(nameof(this.Savegame));
            this.OnHistoryChangedCore(this, EventArgs.Empty);
            this.SetCurrentPage(this.OverviewPage);
        }

        private void BuildNavigation(TGame game)
        {
            this.navigation.Clear();
            this.navigationBuilder.Build(this.navigation, game);
            this.navigation.ReapplyFilter();
        }

        private void OnHistoryChangedCore(object? sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(this.CanUndo));
            this.RaisePropertyChanged(nameof(this.CanRedo));
            this.RebuildNavigationIfStructureChanged();
            this.RefreshPendingChanges();
            this.OnHistoryChanged();
            this.RefreshOverview(this.currentPage);
        }

        private void RefreshOverview(object? page)
        {
            if (page != null && ReferenceEquals(page, this.OverviewPage) && page is IRefreshableViewModel refreshable)
            {
                refreshable.Refresh();
            }
        }

        private void RebuildNavigationIfStructureChanged()
        {
            if (this.currentGame == null)
            {
                return;
            }

            var now = this.navigationBuilder.StructureKey(this.currentGame);
            if (now == this.structureKey)
            {
                return;
            }

            this.structureKey = now;

            // A castle or a hero that changed hands moves to the group of its new owner.
            this.BuildNavigation(this.currentGame);
            if (this.currentPage != null)
            {
                this.SetCurrentPage(this.currentPage);
            }
        }

        private void RefreshPendingChanges()
        {
            var changes = this.currentGame?.History.GetPendingChanges() ?? [];

            // Keep what has not changed, so the list tells the screen about the changes that are different and not about every one.
            var same = 0;
            while (same < changes.Count && same < this.PendingChanges.Count && changes[same].IsSameChangeAs(this.PendingChanges[same]))
            {
                same++;
            }

            while (this.PendingChanges.Count > same)
            {
                this.PendingChanges.RemoveAt(this.PendingChanges.Count - 1);
            }

            for (var i = same; i < changes.Count; i++)
            {
                this.PendingChanges.Add(changes[i]);
            }

            this.RaisePropertyChanged(nameof(this.HasPendingChanges));
            this.navigation.MarkChanges(this.PendingChanges);
            if (this.currentGame != null)
            {
                this.navigationBuilder.RefreshMarks(this.navigation, this.currentGame);
            }
        }
    }
}
