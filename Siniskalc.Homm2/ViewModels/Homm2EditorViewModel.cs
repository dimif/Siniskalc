namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The editor of Heroes of Might and Magic II: the open savegame, its history and what it shows. The application shell
    /// (<see cref="ShellViewModel"/>) works with it through <see cref="IGameEditor"/>.
    /// </summary>
    public sealed class Homm2EditorViewModel : AbstractViewModelBase, IGameEditor
    {
        private readonly IHomm2GameService gameService;
        private readonly ISaveRegionProvider saveRegions;
        private readonly Homm2NavigationTree navigation;
        private readonly Dictionary<int, Homm2PlayerViewModel> players = [];
        private readonly Dictionary<int, Homm2HeroViewModel> heroes = [];
        private readonly Dictionary<int, Homm2CastleViewModel> castles = [];
        private Homm2Game? currentGame;
        private Homm2OverviewViewModel? overview;
        private object? currentPage;
        private string heroFilter = string.Empty;

        // Who owns each castle and hero, as a text. The tree groups them by owner, so it is built again when this changes.
        private string owners = string.Empty;

        public Homm2EditorViewModel(IHomm2GameService gameService, ISaveRegionProvider saveRegions)
        {
            this.gameService = gameService;
            this.saveRegions = saveRegions;
            this.navigation = new Homm2NavigationTree(this.OnNodeSelected);
        }

        /// <summary>
        /// Gets the loaded game, or <c>null</c> when none is loaded.
        /// </summary>
        public Homm2Game? CurrentGame => this.currentGame;

        /// <summary>
        /// Gets what the editor shows about the loaded game, or <c>null</c> when none is loaded.
        /// </summary>
        public Homm2OverviewViewModel? Overview => this.overview;

        /// <summary>
        /// Gets what the editor shows: a <see cref="Homm2HeroViewModel"/>, a <see cref="Homm2PlayerViewModel"/>, a
        /// <see cref="Homm2CastleViewModel"/>, the <see cref="Homm2OverviewViewModel"/>, or nothing while no savegame is loaded.
        /// </summary>
        public object? CurrentPage => this.currentPage;

        /// <summary>
        /// Gets the top level nodes of the navigation tree.
        /// </summary>
        public ObservableCollection<Homm2NavNodeViewModel> NavigationNodes => this.navigation.Nodes;

        /// <summary>
        /// Gets or sets the text the navigation tree is filtered by.
        /// </summary>
        public string HeroFilter
        {
            get => this.heroFilter;
            set
            {
                value ??= string.Empty;
                if (this.heroFilter == value)
                {
                    return;
                }

                this.heroFilter = value;
                this.RaisePropertyChanged(nameof(this.HeroFilter));
                this.navigation.ApplyFilter(value);
            }
        }

        /// <inheritdoc/>
        public bool IsGameLoaded => this.currentGame != null;

        /// <inheritdoc/>
        public string? File => this.currentGame?.File;

        /// <inheritdoc/>
        public string? Name => this.currentGame?.Name;

        /// <inheritdoc/>
        public bool HasFile => !string.IsNullOrWhiteSpace(this.currentGame?.File);

        /// <inheritdoc/>
        public bool CanReload => this.HasFile;

        /// <inheritdoc/>
        public bool CanUndo => this.currentGame?.History.CanUndo ?? false;

        /// <inheritdoc/>
        public bool CanRedo => this.currentGame?.History.CanRedo ?? false;

        /// <inheritdoc/>
        public ObservableCollection<PendingChange> PendingChanges { get; } = new();

        /// <inheritdoc/>
        public bool HasPendingChanges => this.PendingChanges.Count > 0;

        /// <inheritdoc/>
        public void LoadGame(string filePath) => this.SetGame(this.gameService.LoadGameFromFile(filePath));

        /// <inheritdoc/>
        public void LoadGameFromBytes(byte[] bytes, string? sourceName = null) => this.SetGame(this.gameService.LoadGameFromBytes(bytes, sourceName));

        /// <inheritdoc/>
        public void ReloadGame()
        {
            if (this.currentGame == null)
            {
                throw new InvalidOperationException("No game is currently loaded");
            }

            this.SetGame(this.gameService.ReloadGame(this.currentGame));
        }

        /// <inheritdoc/>
        public void SaveGame()
        {
            var file = this.currentGame?.File;
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new InvalidOperationException("The game has no file to save to");
            }

            this.SaveGame(file);
        }

        /// <inheritdoc/>
        public void SaveGame(string filePath)
        {
            if (this.currentGame == null)
            {
                throw new InvalidOperationException("No game is currently loaded");
            }

            this.gameService.SaveGame(this.currentGame, filePath);
            this.RaiseFileChanged();
        }

        /// <inheritdoc/>
        public byte[] GetBytesToSave() => this.Loaded().GetBytesToSave();

        /// <inheritdoc/>
        public SaveHexDump CreateSaveHexDump()
        {
            var game = this.Loaded();
            var name = string.IsNullOrWhiteSpace(game.Name) ? "Savefile" : System.IO.Path.GetFileName(game.Name);
            return SaveHexDump.Create($"{name} ({game.GameVersion})", game.History, this.saveRegions);
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
                this.currentGame.History.Apply(this.currentGame.History.Original);
            }
        }

        /// <summary>
        /// Shows the overview of the savegame.
        /// </summary>
        public void ShowOverview() => this.SetCurrentPage(this.overview);

        /// <summary>
        /// Shows the page of the player of <paramref name="color"/>, on the tab <paramref name="tab"/>.
        /// </summary>
        public void ShowPlayer(Homm2Color color, int tab = 0)
        {
            var game = this.Loaded();
            if (!this.players.TryGetValue((int)color, out var player))
            {
                player = new Homm2PlayerViewModel(game, color, this.ShowHero, this.ShowCastle);
                this.players[(int)color] = player;
            }

            player.SelectedTab = tab;
            this.SetCurrentPage(player);
        }

        /// <summary>
        /// Shows the page of the hero with the number <paramref name="heroId"/>.
        /// </summary>
        public void ShowHero(int heroId)
        {
            var game = this.Loaded();
            if (!this.heroes.TryGetValue(heroId, out var hero))
            {
                hero = new Homm2HeroViewModel(game, heroId, this.ShowCastle);
                this.heroes[heroId] = hero;
            }

            this.SetCurrentPage(hero);
        }

        /// <summary>
        /// Shows the page of the castle at <paramref name="castleIndex"/> in the savegame's castles.
        /// </summary>
        public void ShowCastle(int castleIndex)
        {
            var game = this.Loaded();
            if (!this.castles.TryGetValue(castleIndex, out var castle))
            {
                castle = new Homm2CastleViewModel(game, castleIndex, this.ShowHero);
                this.castles[castleIndex] = castle;
            }

            this.SetCurrentPage(castle);
        }

        private static string OwnersOf(Homm2Game game)
        {
            var document = game.History.Current;
            return string.Join(",", document.Castles().GetOwners().Select(o => o?.ToString() ?? "-"))
                + ";" + string.Join(",", document.Heroes().GetOwners().Select(o => o?.ToString() ?? "-"));
        }

        private Homm2Game Loaded() => this.currentGame ?? throw new InvalidOperationException("No game is currently loaded");

        private void SetGame(Homm2Game game)
        {
            // What reads the new game (and so can fail on a savefile that is odd) is built before anything of the open game is let go.
            var newOverview = new Homm2OverviewViewModel(game, color => this.ShowPlayer(color));
            var newOwners = OwnersOf(game);

            this.DisposePages();

            if (this.currentGame != null)
            {
                this.currentGame.History.Changed -= this.OnHistoryChanged;
            }

            this.overview?.Dispose();
            this.currentPage = null;
            this.currentGame = game;
            this.overview = newOverview;
            this.owners = newOwners;
            this.currentGame.History.Changed += this.OnHistoryChanged;
            this.navigation.Build(game);
            this.PendingChanges.Clear();
            this.RaisePropertyChanged(nameof(this.CurrentGame));
            this.RaisePropertyChanged(nameof(this.Overview));
            this.RaisePropertyChanged(nameof(this.IsGameLoaded));
            this.RaiseFileChanged();
            this.OnHistoryChanged(this, EventArgs.Empty);
            this.SetCurrentPage(this.overview);
        }

        private void DisposePages()
        {
            foreach (var page in this.players.Values.Cast<Homm2PageViewModel>().Concat(this.heroes.Values).Concat(this.castles.Values))
            {
                page.Dispose();
            }

            this.players.Clear();
            this.heroes.Clear();
            this.castles.Clear();
        }

        private void SetCurrentPage(object? page)
        {
            if (!ReferenceEquals(this.currentPage, page))
            {
                this.currentPage = page;
                this.RaisePropertyChanged(nameof(this.CurrentPage));
            }

            switch (page)
            {
                case Homm2OverviewViewModel:
                    this.navigation.Reveal(this.navigation.Overview);
                    break;
                case Homm2PlayerViewModel player:
                    this.navigation.Reveal(this.navigation.PlayerTabNode((int)player.Color, player.SelectedTab));
                    break;
                case Homm2HeroViewModel hero:
                    this.navigation.Reveal(this.navigation.HeroNode(hero.HeroId));
                    break;
                case Homm2CastleViewModel castle:
                    this.navigation.Reveal(this.navigation.CastleNode(castle.CastleIndex));
                    break;
            }
        }

        private void OnNodeSelected(Homm2NavNodeViewModel node)
        {
            switch (node.Kind)
            {
                case Homm2NavNodeKind.Overview:
                    this.ShowOverview();
                    break;
                case Homm2NavNodeKind.Player:
                    this.ShowPlayer((Homm2Color)node.Index);
                    break;
                case Homm2NavNodeKind.PlayerTab:
                    this.ShowPlayer((Homm2Color)node.Index, node.Tab);
                    break;
                case Homm2NavNodeKind.Hero:
                    this.ShowHero(node.Index);
                    break;
                case Homm2NavNodeKind.Castle:
                    this.ShowCastle(node.Index);
                    break;
            }
        }

        private void RaiseFileChanged()
        {
            this.RaisePropertyChanged(nameof(this.File));
            this.RaisePropertyChanged(nameof(this.Name));
            this.RaisePropertyChanged(nameof(this.HasFile));
            this.RaisePropertyChanged(nameof(this.CanReload));
        }

        private void OnHistoryChanged(object? sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(this.CanUndo));
            this.RaisePropertyChanged(nameof(this.CanRedo));
            this.RebuildNavigationIfOwnersChanged();
            this.RefreshPendingChanges();
        }

        private void RebuildNavigationIfOwnersChanged()
        {
            if (this.currentGame == null)
            {
                return;
            }

            var now = OwnersOf(this.currentGame);
            if (now == this.owners)
            {
                return;
            }

            this.owners = now;

            // A castle or a hero that changed hands moves to the group of its new owner.
            this.navigation.Build(this.currentGame);
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
                this.navigation.RefreshVisitors(this.currentGame);
            }
        }
    }
}
