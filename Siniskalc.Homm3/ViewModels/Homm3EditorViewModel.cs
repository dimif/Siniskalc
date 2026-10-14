namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Homm3.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The editor of Heroes of Might and Magic III: the open savegame, its navigation tree and the pages (hero, player, castle,
    /// overview) it shows. Orchestrates game loading, hero management, and hero selection. The application shell
    /// (<see cref="ShellViewModel"/>) works with it through <see cref="IGameEditor"/>.
    /// </summary>
    public class Homm3EditorViewModel : EditorViewModelBase<Game>
    {
        private readonly EffectLineProvider effectLines;
        private readonly BanishingTextProvider banishingText;
        private readonly CastleBuildMessageProvider castleBuildMessages;
        private readonly ISpellPointsCalculator spellPoints;
        private readonly IByteEditTargetFactory byteEditTargets;
        private readonly Dictionary<int, PlayerViewModel> players = new Dictionary<int, PlayerViewModel>();
        private readonly Dictionary<int, CastleViewModel> castles = new Dictionary<int, CastleViewModel>();
        private OverviewViewModel? overview;
        private HeroViewModel? selectedHeroViewModel;

        public Homm3EditorViewModel(IGameService gameService, EffectLineProvider effectLines, BanishingTextProvider banishingText, CastleBuildMessageProvider castleBuildMessages, ISpellPointsCalculator spellPoints, ISaveRegionProvider saveRegions, IByteEditTargetFactory byteEditTargets, INavigationBuilder<Game> navigationBuilder)
            : base(gameService, saveRegions, navigationBuilder)
        {
            this.effectLines = effectLines;
            this.banishingText = banishingText;
            this.castleBuildMessages = castleBuildMessages;
            this.spellPoints = spellPoints;
            this.byteEditTargets = byteEditTargets;
            this.Heroes = new ObservableCollection<HeroViewModel>();
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
                }
                else if (this.CurrentPage is HeroViewModel)
                {
                    this.SetCurrentPage(this.overview);
                }
            }
        }

        /// <summary>
        /// Gets the overview of the loaded savegame, or <c>null</c> when none is loaded.
        /// </summary>
        public OverviewViewModel? Overview => this.overview;

        /// <inheritdoc/>
        protected override object? OverviewPage => this.overview;

        /// <summary>
        /// Reloads the current game from its file, discarding unsaved edits.
        /// Reopens the heroes that were open, and keeps the selected hero if possible.
        /// </summary>
        public override void ReloadGame()
        {
            var openHeroNames = this.Heroes.Select(hvm => hvm.Name).ToList();
            var selectedHeroName = this.selectedHeroViewModel?.Name;

            base.ReloadGame();

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
            this.Loaded();

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
        /// Shows the page of the player at <paramref name="playerIndex"/> in the savegame's players.
        /// </summary>
        public void ShowPlayer(int playerIndex, int tab = 0)
        {
            var game = this.Loaded();

            if (!this.players.TryGetValue(playerIndex, out var player))
            {
                var color = game.History.Current.Players().GetColor(playerIndex);
                player = new PlayerViewModel(
                    game.History.CreatePlayerSession(playerIndex),
                    this.byteEditTargets,
                    this.banishingText,
                    game.GetControl(color),
                    game.CurrentPlayer == color,
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
            var game = this.Loaded();

            if (!this.castles.TryGetValue(castleIndex, out var castle))
            {
                castle = new CastleViewModel(game.History, castleIndex, this.byteEditTargets, this.castleBuildMessages, this.SearchAndSelectHero);
                this.castles[castleIndex] = castle;
            }

            this.SetCurrentPage(castle);
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

        /// <inheritdoc/>
        protected override object? PrepareGame(Game game) =>
            new OverviewViewModel(game, this.banishingText, index => this.SearchAndSelectHero(index), index => this.ShowPlayer(index), this.ShowCastle);

        /// <inheritdoc/>
        protected override void CloseGame()
        {
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
        }

        /// <inheritdoc/>
        protected override void OpenGame(Game game, object? prepared)
        {
            this.overview = (OverviewViewModel?)prepared;
            this.RaisePropertyChanged(nameof(this.Overview));
        }

        /// <inheritdoc/>
        protected override NavNodeViewModel? NodeOf(object page) => page switch
        {
            OverviewViewModel => this.Navigation.Overview,
            PlayerViewModel player => this.Navigation.PlayerTabNode(player.Session.PlayerIndex, player.SelectedTab),
            HeroViewModel hero => this.Navigation.HeroNode(hero.Session.HeroIndex),
            CastleViewModel castle => this.Navigation.CastleNode(castle.CastleIndex),
            _ => null,
        };

        /// <inheritdoc/>
        protected override void OnNodeSelected(NavNodeViewModel node)
        {
            switch (node.Kind)
            {
                case NavNodeKind.Hero:
                    this.SelectedHeroViewModel = this.OpenHeroAt(node.Index);
                    break;
                case NavNodeKind.Player:
                    this.ShowPlayer(node.Index);
                    break;
                case NavNodeKind.PlayerTab:
                    this.ShowPlayer(node.Index, node.Tab);
                    break;
                case NavNodeKind.Castle:
                    this.ShowCastle(node.Index);
                    break;
                case NavNodeKind.Overview:
                    this.ShowOverview();
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void OnPageShowing(object? page)
        {
            if (page is OverviewViewModel shown && ReferenceEquals(shown, this.overview))
            {
                // It was not kept up to date while it was out of sight.
                shown.Refresh();
            }
        }

        /// <inheritdoc/>
        protected override void OnHistoryChanged()
        {
            if (this.overview != null && ReferenceEquals(this.CurrentPage, this.overview))
            {
                this.overview.Refresh();
            }
        }

        private void SearchAndSelectHero(int heroIndex)
        {
            this.SelectedHeroViewModel = this.OpenHeroAt(heroIndex);
        }

        private HeroViewModel? OpenHero(string heroName)
        {
            var open = this.Heroes.FirstOrDefault(hvm => string.Equals(hvm.Name, heroName, StringComparison.OrdinalIgnoreCase));
            if (open != null)
            {
                return open;
            }

            if (this.CurrentGame == null)
            {
                return null;
            }

            var heroIndex = this.CurrentGame.History.Current.Heroes().FindByName(heroName);
            return heroIndex < 0 ? null : this.OpenHeroAt(heroIndex);
        }

        private HeroViewModel? OpenHeroAt(int heroIndex)
        {
            if (this.CurrentGame == null)
            {
                return null;
            }

            var open = this.Heroes.FirstOrDefault(hvm => hvm.Session.HeroIndex == heroIndex);
            if (open != null)
            {
                return open;
            }

            var heroViewModel = new HeroViewModel(this.CurrentGame.History.CreateHeroSession(heroIndex), this.effectLines, this.spellPoints, this.byteEditTargets, this.CurrentGame.Map?.HasUnderground ?? true, this.ShowCastle);
            this.Heroes.Add(heroViewModel);
            return heroViewModel;
        }
    }
}
