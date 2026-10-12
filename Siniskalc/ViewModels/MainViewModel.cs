namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Data;
    using Siniskalc.Models;
    using Siniskalc.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Root ViewModel that manages the application state.
    /// Orchestrates game loading, hero management, and hero selection.
    /// </summary>
    public class MainViewModel : AbstractViewModelBase
    {
        private readonly IGameService gameService;

        private readonly EffectLineProvider effectLines;
        private readonly CastleBuildMessageProvider castleBuildMessages;
        private readonly ISpellPointsCalculator spellPoints;

        private readonly NavigationTree navigation;

        private readonly ISaveRegionProvider saveRegions;

        private readonly IByteEditTargetFactory byteEditTargets;

        private readonly SettingsStore settings;

        private readonly Dictionary<int, PlayerViewModel> players = new Dictionary<int, PlayerViewModel>();

        private readonly Dictionary<int, CastleViewModel> castles = new Dictionary<int, CastleViewModel>();

        private OverviewViewModel? overview;

        // Who owns each castle, as a text. The tree groups castles by owner, so it is built again when this changes.
        private string castleOwners = string.Empty;

        private object? currentPage;

        private HeroViewModel? selectedHeroViewModel;

        private string heroFilter = string.Empty;

        private bool isPendingChangesOpen;

        private Game? currentGame;

        public MainViewModel(IGameService gameService, EffectLineProvider effectLines, CastleBuildMessageProvider castleBuildMessages, ISpellPointsCalculator spellPoints, ISaveRegionProvider saveRegions, IByteEditTargetFactory byteEditTargets, SettingsStore settings)
        {
            this.settings = settings;
            this.gameService = gameService;
            this.effectLines = effectLines;
            this.castleBuildMessages = castleBuildMessages;
            this.spellPoints = spellPoints;
            this.saveRegions = saveRegions;
            this.byteEditTargets = byteEditTargets;
            this.navigation = new NavigationTree(this.OnNodeSelected);
            this.Heroes = new ObservableCollection<HeroViewModel>();
            this.RevertChangeCommand = new DelegateCommand(p => this.RevertChange((PendingChange?)p), p => p is PendingChange);
            this.RevertAllCommand = new DelegateCommand(_ => this.RevertAll(), _ => this.HasPendingChanges);
        }

        /// <summary>
        /// Gets or sets whether a copy of a savefile is kept before it is saved over. Kept in the settings of the user.
        /// </summary>
        public bool BackupOnSave
        {
            get => this.settings.BackupOnSave;
            set
            {
                if (this.settings.BackupOnSave != value)
                {
                    this.settings.BackupOnSave = value;
                    this.RaisePropertyChanged(nameof(this.BackupOnSave));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the Advanced menu and the hex tabs are shown. Kept in the settings of the user.
        /// </summary>
        public bool IsAdvancedMode
        {
            get => this.settings.AdvancedMode;
            set
            {
                if (this.settings.AdvancedMode != value)
                {
                    this.settings.AdvancedMode = value;
                    this.RaisePropertyChanged(nameof(this.IsAdvancedMode));
                }
            }
        }

        /// <summary>
        /// Gets the collection of open HeroViewModels.
        /// </summary>
        public ObservableCollection<HeroViewModel> Heroes { get; }

        /// <summary>
        /// Gets or sets the currently selected HeroViewModel.
        /// When set, raises PropertyChanged notification for UI binding.
        /// </summary>
        public HeroViewModel? SelectedHeroViewModel
        {
            get => this.selectedHeroViewModel;
            set
            {
                if (this.selectedHeroViewModel != value)
                {
                    this.selectedHeroViewModel = value;
                    this.RaisePropertyChanged(nameof(this.SelectedHeroViewModel));
                }

                if (value != null)
                {
                    this.SetCurrentPage(value);
                    this.navigation.Reveal(this.navigation.HeroNode(value.Session.HeroIndex));
                }
                else if (this.currentPage is HeroViewModel)
                {
                    this.SetCurrentPage(this.overview);
                }
            }
        }

        /// <summary>
        /// Gets what the editor shows: a <see cref="HeroViewModel"/>, a <see cref="PlayerViewModel"/>, a
        /// <see cref="CastleViewModel"/>, the <see cref="OverviewViewModel"/>, or nothing while no savegame is loaded.
        /// </summary>
        public object? CurrentPage => this.currentPage;

        /// <summary>
        /// Gets the overview of the loaded savegame, or <c>null</c> when none is loaded.
        /// </summary>
        public OverviewViewModel? Overview => this.overview;

        /// <summary>
        /// Gets the top level nodes of the navigation tree.
        /// </summary>
        public ObservableCollection<NavNodeViewModel> NavigationNodes => this.navigation.Nodes;

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

        /// <summary>
        /// Gets or sets whether the pending changes panel is shown.
        /// </summary>
        public bool IsPendingChangesOpen
        {
            get => this.isPendingChangesOpen;
            set
            {
                if (this.isPendingChangesOpen != value)
                {
                    this.isPendingChangesOpen = value;
                    this.RaisePropertyChanged(nameof(this.IsPendingChangesOpen));
                }
            }
        }

        public ICommand RevertChangeCommand { get; private set; }

        public ICommand RevertAllCommand { get; private set; }

        /// <summary>
        /// Gets the currently loaded Game model.
        /// </summary>
        public Game? CurrentGame => this.currentGame;

        /// <summary>
        /// Gets a value indicating whether a game is currently loaded.
        /// </summary>
        public bool IsGameLoaded => this.currentGame != null;

        /// <summary>
        /// Gets what has been edited since the savefile was loaded. Updated after every edit, undo and redo.
        /// </summary>
        public ObservableCollection<PendingChange> PendingChanges { get; } = new ObservableCollection<PendingChange>();

        public bool HasPendingChanges => this.PendingChanges.Count > 0;

        public bool CanUndo => this.currentGame?.History.CanUndo ?? false;

        public bool CanRedo => this.currentGame?.History.CanRedo ?? false;

        /// <summary>
        /// Gets a value indicating whether the game is tied to a file: where it was loaded from or last saved to. A game loaded from a dump
        /// is not, until it is saved.
        /// </summary>
        public bool HasFile => !string.IsNullOrWhiteSpace(this.currentGame?.File);

        /// <summary>
        /// Gets a value indicating whether the game can be read again from its file.
        /// </summary>
        public bool CanReload => this.HasFile;

        /// <summary>
        /// Loads a game from the specified file path. Clears the open heroes.
        /// </summary>
        /// <param name="filePath">Path to the savegame file</param>
        public void LoadGame(string filePath)
        {
            this.SetGame(this.gameService.LoadGameFromFile(filePath));
        }

        /// <summary>
        /// Loads a game from raw bytes (e.g., from a dump file). Clears the open heroes.
        /// </summary>
        /// <param name="bytes">Raw game data</param>
        /// <param name="sourceName">Optional source name for tracking</param>
        public void LoadGameFromBytes(byte[] bytes, string? sourceName = null)
        {
            this.SetGame(this.gameService.LoadGameFromBytes(bytes, sourceName));
        }

        /// <summary>
        /// Reloads the current game from its file, discarding unsaved edits.
        /// Reopens the heroes that were open, and keeps the selected hero if possible.
        /// </summary>
        public void ReloadGame()
        {
            if (this.currentGame == null)
            {
                throw new InvalidOperationException("No game is currently loaded");
            }

            var openHeroNames = this.Heroes.Select(hvm => hvm.Name).ToList();
            var selectedHeroName = this.selectedHeroViewModel?.Name;

            this.SetGame(this.gameService.ReloadGame(this.currentGame));

            foreach (var name in openHeroNames)
            {
                this.OpenHero(name);
            }

            this.SelectedHeroViewModel = this.Heroes.FirstOrDefault(hvm => hvm.Name == selectedHeroName)
                ?? this.Heroes.FirstOrDefault();
        }

        /// <summary>
        /// Searches for a hero by name in the current game.
        /// If found, opens the hero (or selects it, if already open).
        /// </summary>
        /// <param name="heroName">The name of the hero to search for</param>
        /// <returns>True if the hero was found; false otherwise</returns>
        public bool SearchAndAddHero(string heroName)
        {
            if (this.currentGame == null)
            {
                throw new InvalidOperationException("No game is currently loaded");
            }

            if (string.IsNullOrWhiteSpace(heroName))
            {
                return false;
            }

            var heroViewModel = this.OpenHero(heroName);
            if (heroViewModel != null)
            {
                this.SelectedHeroViewModel = heroViewModel;
            }

            return heroViewModel != null;
        }

        /// <summary>
        /// Shows the overview of the savegame.
        /// </summary>
        public void ShowOverview()
        {
            this.SetCurrentPage(this.overview);
        }

        /// <summary>
        /// Shows the page of the player at <paramref name="playerIndex"/> in the savegame's players.
        /// </summary>
        public void ShowPlayer(int playerIndex, int tab = 0)
        {
            if (this.currentGame == null)
            {
                throw new InvalidOperationException("No game is currently loaded");
            }

            if (!this.players.TryGetValue(playerIndex, out var player))
            {
                var color = this.currentGame.History.Current.Players().GetColor(playerIndex);
                player = new PlayerViewModel(
                    this.currentGame.History.CreatePlayerSession(playerIndex),
                    this.byteEditTargets,
                    this.currentGame.GetControl(color),
                    this.currentGame.CurrentPlayer == color,
                    this.SearchAndSelectHero,
                    this.ShowCastle);
                this.players[playerIndex] = player;
            }

            player.SelectedTab = tab;

            this.SetCurrentPage(player);
        }

        /// <summary>
        /// Shows the page of the castle at <paramref name="castleIndex"/> in the savegame's castles.
        /// </summary>
        public void ShowCastle(int castleIndex)
        {
            if (this.currentGame == null)
            {
                throw new InvalidOperationException("No game is currently loaded");
            }

            if (!this.castles.TryGetValue(castleIndex, out var castle))
            {
                castle = new CastleViewModel(this.currentGame.History, castleIndex, this.byteEditTargets, this.castleBuildMessages, this.SearchAndSelectHero);
                this.castles[castleIndex] = castle;
            }

            this.SetCurrentPage(castle);
        }

        /// <summary>
        /// Takes the bytes of the whole savefile as they would be saved, with its known regions, for the hex view.
        /// </summary>
        public SaveHexDump CreateSaveHexDump()
        {
            if (this.currentGame == null)
            {
                throw new InvalidOperationException("No game is currently loaded");
            }

            var name = string.IsNullOrWhiteSpace(this.currentGame.Name) ? "Savefile" : System.IO.Path.GetFileName(this.currentGame.Name);
            return SaveHexDump.Create($"{name} ({this.currentGame.GameVersion})", this.currentGame.History, this.saveRegions);
        }

        /// <summary>
        /// Closes a hero. Its edits stay in the game's history.
        /// </summary>
        /// <param name="heroViewModel">The HeroViewModel to remove</param>
        public void RemoveHero(HeroViewModel heroViewModel)
        {
            if (!this.Heroes.Remove(heroViewModel))
            {
                return;
            }

            heroViewModel.Dispose();

            // Clear selection if the removed hero was selected
            if (this.SelectedHeroViewModel == heroViewModel)
            {
                this.SelectedHeroViewModel = this.Heroes.FirstOrDefault();
            }
        }

        /// <summary>
        /// Closes all heroes and resets the selection.
        /// </summary>
        public void ClearHeroes()
        {
            foreach (var heroViewModel in this.Heroes)
            {
                heroViewModel.Dispose();
            }

            this.Heroes.Clear();
            this.SelectedHeroViewModel = null;
        }

        /// <summary>
        /// Puts one pending change back to its loaded value. It can be undone.
        /// </summary>
        public void RevertChange(PendingChange? change)
        {
            if (this.currentGame != null)
            {
                change?.Revert(this.currentGame.History);
            }
        }

        /// <summary>
        /// Puts every edit back to the loaded values. It can be undone.
        /// </summary>
        public void RevertAll()
        {
            if (this.currentGame != null && this.HasPendingChanges)
            {
                this.currentGame.History.Apply(this.currentGame.History.Original);
            }
        }

        public void Undo()
        {
            this.currentGame?.History.Undo();
        }

        public void Redo()
        {
            this.currentGame?.History.Redo();
        }

        /// <summary>
        /// Saves the current game over the file it was loaded from or last saved to.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no game is loaded or the game has no file yet (use <see cref="SaveGame(string)"/>)</exception>
        public void SaveGame()
        {
            var file = this.currentGame?.File;
            if (string.IsNullOrWhiteSpace(file))
            {
                throw new InvalidOperationException("The game has no file to save to");
            }

            this.SaveGame(file);
        }

        /// <summary>
        /// Saves the current game to the specified file path.
        /// </summary>
        /// <param name="filePath">Path where the game should be saved</param>
        public void SaveGame(string filePath)
        {
            if (this.currentGame == null)
            {
                throw new InvalidOperationException("No game is currently loaded");
            }

            this.gameService.SaveGame(this.currentGame, filePath);
        }

        /// <summary>
        /// Returns who owns each castle and hero, as one text that changes when any of them changes hands. Reads the owner bytes only.
        /// </summary>
        private static string OwnersOf(Game game)
        {
            var document = game.History.Current;
            return string.Join(",", document.Castles().GetOwners().Select(o => o?.ToString() ?? "-"))
                + ";" + string.Join(",", document.Heroes().GetOwners().Select(o => o?.ToString() ?? "-"));
        }

        private void SetGame(Game game)
        {
            // What reads the new game (and so can fail on a savefile that is odd) is built before anything of the open game is let go,
            // so a game that cannot be shown leaves the one that is open as it was.
            var newOverview = new OverviewViewModel(game, index => this.SearchAndSelectHero(index), index => this.ShowPlayer(index), this.ShowCastle);
            var newOwners = OwnersOf(game);

            this.ClearHeroes();

            foreach (var player in this.players.Values)
            {
                player.Dispose();
            }

            this.players.Clear();

            foreach (var castle in this.castles.Values)
            {
                castle.Dispose();
            }

            this.castles.Clear();
            this.currentPage = null;

            if (this.currentGame != null)
            {
                this.currentGame.History.Changed -= this.OnHistoryChanged;
            }

            this.currentGame = game;
            this.currentGame.History.Changed += this.OnHistoryChanged;
            this.overview = newOverview;
            this.castleOwners = newOwners;
            this.navigation.Build(game);
            this.RaisePropertyChanged(nameof(this.Overview));

            this.RaisePropertyChanged(nameof(this.CurrentGame));
            this.RaisePropertyChanged(nameof(this.IsGameLoaded));
            this.PendingChanges.Clear();
            this.OnHistoryChanged(this, EventArgs.Empty);
            this.SetCurrentPage(this.overview);
        }

        private void SearchAndSelectHero(int heroIndex)
        {
            this.SelectedHeroViewModel = this.OpenHeroAt(heroIndex);
        }

        private void SetCurrentPage(object? page)
        {
            if (!ReferenceEquals(this.currentPage, page))
            {
                if (page is OverviewViewModel shown && ReferenceEquals(shown, this.overview))
                {
                    // It was not kept up to date while it was out of sight.
                    shown.Refresh();
                }

                this.currentPage = page;
                this.RaisePropertyChanged(nameof(this.CurrentPage));
            }

            if (ReferenceEquals(page, this.overview))
            {
                this.navigation.Reveal(this.navigation.Overview);
            }
            else if (page is PlayerViewModel player)
            {
                this.navigation.Reveal(this.navigation.PlayerTabNode(player.Session.PlayerIndex, player.SelectedTab));
            }
            else if (page is CastleViewModel castle)
            {
                this.navigation.Reveal(this.navigation.CastleNode(castle.CastleIndex));
            }
        }

        private HeroViewModel? OpenHero(string heroName)
        {
            var open = this.Heroes.FirstOrDefault(hvm => string.Equals(hvm.Name, heroName, StringComparison.OrdinalIgnoreCase));
            if (open != null)
            {
                return open;
            }

            if (this.currentGame == null)
            {
                return null;
            }

            var heroIndex = this.currentGame.History.Current.Heroes().FindByName(heroName);
            return heroIndex < 0 ? null : this.OpenHeroAt(heroIndex);
        }

        private HeroViewModel? OpenHeroAt(int heroIndex)
        {
            if (this.currentGame == null)
            {
                return null;
            }

            var open = this.Heroes.FirstOrDefault(hvm => hvm.Session.HeroIndex == heroIndex);
            if (open != null)
            {
                return open;
            }

            var heroViewModel = new HeroViewModel(this.currentGame.History.CreateHeroSession(heroIndex), this.effectLines, this.spellPoints, this.byteEditTargets, this.currentGame.Map?.HasUnderground ?? true, this.ShowCastle);
            this.Heroes.Add(heroViewModel);
            return heroViewModel;
        }

        private void OnNodeSelected(NavNodeViewModel node)
        {
            switch (node.Kind)
            {
                case NavNodeKind.Hero:
                    this.SelectedHeroViewModel = this.OpenHeroAt(node.HeroIndex);
                    break;
                case NavNodeKind.Player:
                    this.ShowPlayer(node.PlayerIndex);
                    break;
                case NavNodeKind.PlayerTab:
                    this.ShowPlayer(node.PlayerIndex, node.Tab);
                    break;
                case NavNodeKind.Castle:
                    this.ShowCastle(node.CastleIndex);
                    break;
                case NavNodeKind.Overview:
                    this.ShowOverview();
                    break;
            }
        }

        private void OnHistoryChanged(object? sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(this.CanUndo));
            this.RaisePropertyChanged(nameof(this.CanRedo));
            this.RebuildNavigationIfOwnersChanged();
            this.RefreshPendingChanges();

            if (this.overview != null && ReferenceEquals(this.currentPage, this.overview))
            {
                this.overview.Refresh();
            }
        }

        private void RebuildNavigationIfOwnersChanged()
        {
            if (this.currentGame == null)
            {
                return;
            }

            var owners = OwnersOf(this.currentGame);
            if (owners == this.castleOwners)
            {
                return;
            }

            this.castleOwners = owners;

            // A castle that changed hands moves to the group of its new owner.
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
                this.navigation.RefreshCastleMarks(this.currentGame);
            }

            (this.RevertAllCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }
    }
}
