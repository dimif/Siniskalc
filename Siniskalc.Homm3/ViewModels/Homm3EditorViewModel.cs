namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Homm3.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The editor of Heroes of Might and Magic III: the open savegame, its navigation tree and the pages (hero, player, castle,
    /// overview) it shows. The application shell (<see cref="ShellViewModel"/>) works with it through <see cref="IGameEditor"/>.
    /// </summary>
    public sealed class Homm3EditorViewModel : EditorViewModelBase<Game>
    {
        private readonly EffectLineProvider effectLines;
        private readonly BanishingTextProvider banishingText;
        private readonly CastleBuildMessageProvider castleBuildMessages;
        private readonly ISpellPointsCalculatorFactory spellPoints;
        private readonly IByteEditTargetFactory byteEditTargets;
        private readonly Dictionary<int, PlayerViewModel> players = [];
        private readonly Dictionary<int, HeroViewModel> heroes = [];
        private readonly Dictionary<int, CastleViewModel> castles = [];
        private MapViewModel? map;
        private NeutralViewModel? neutral;
        private OverviewViewModel? overview;

        public Homm3EditorViewModel(IGameService gameService, EffectLineProvider effectLines, BanishingTextProvider banishingText, CastleBuildMessageProvider castleBuildMessages, ISpellPointsCalculatorFactory spellPoints, ISaveRegionProvider saveRegions, IByteEditTargetFactory byteEditTargets, INavigationBuilder<Game> navigationBuilder)
            : base(gameService, saveRegions, navigationBuilder)
        {
            this.effectLines = effectLines;
            this.banishingText = banishingText;
            this.castleBuildMessages = castleBuildMessages;
            this.spellPoints = spellPoints;
            this.byteEditTargets = byteEditTargets;
        }

        /// <summary>
        /// Gets the overview of the loaded savegame, or <c>null</c> when none is loaded.
        /// </summary>
        public OverviewViewModel? Overview => this.overview;

        /// <inheritdoc/>
        protected override object? OverviewPage => this.overview;

        /// <summary>
        /// Shows the page of the hero with the given name, if the savegame has one.
        /// </summary>
        /// <param name="heroName">The name of the hero to search for.</param>
        /// <returns>True if the hero was found; false otherwise.</returns>
        public bool ShowHero(string heroName)
        {
            var game = this.Loaded();
            if (string.IsNullOrWhiteSpace(heroName))
            {
                return false;
            }

            var heroIndex = game.History.Current.Heroes().FindByName(heroName);
            if (heroIndex < 0)
            {
                return false;
            }

            this.ShowHero(heroIndex);
            return true;
        }

        /// <summary>
        /// Shows the page of the hero at <paramref name="heroIndex"/> in the savegame's heroes.
        /// </summary>
        public void ShowHero(int heroIndex)
        {
            var game = this.Loaded();

            if (!this.heroes.TryGetValue(heroIndex, out var hero))
            {
                hero = new HeroViewModel(game.History.CreateHeroSession(heroIndex), this.effectLines, this.spellPoints.Create(game.History.Current.GameVersion()), this.byteEditTargets, game.Map?.HasUnderground ?? true, this.ShowCastle, game.Places);
                this.heroes[heroIndex] = hero;
            }

            this.SetCurrentPage(hero);
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
                    this.ShowHero,
                    this.ShowCastle);
                this.players[playerIndex] = player;
            }

            player.SelectedTab = tab;

            this.SetCurrentPage(player);
        }

        /// <summary>
        /// Shows the page of what nobody owns, on the tab <paramref name="tab"/> (see <see cref="NeutralTabs"/>).
        /// </summary>
        public void ShowNeutral(int tab = NeutralTabs.Heroes)
        {
            var game = this.Loaded();
            this.neutral ??= new NeutralViewModel(game.History, this.byteEditTargets, this.ShowHero, this.ShowCastle);
            this.neutral.SelectedTab = tab;
            this.SetCurrentPage(this.neutral);
        }

        /// <summary>
        /// Shows the page of what the map keeps that no player owns.
        /// </summary>
        public void ShowMap()
        {
            this.map ??= new MapViewModel(this.Loaded(), this.effectLines);
            this.SetCurrentPage(this.map);
        }

        /// <summary>
        /// Shows the mine at <paramref name="mineIndex"/> among the mines of the savegame: on the Mines tab of its owner, or of Neutral.
        /// </summary>
        public void ShowMine(int mineIndex)
        {
            var game = this.Loaded();
            var owner = game.History.Current.Mines().GetModel(mineIndex).Owner;
            if (owner.IsNeutral)
            {
                this.ShowNeutral(NeutralTabs.Mines);
                this.neutral!.MapObjects.PickMine(mineIndex);
                return;
            }

            var playerIndex = game.History.Current.Players().FindByColor(owner);
            if (playerIndex < 0)
            {
                // TODO:LOG Warning: a mine owned by a colour that has no player in the savegame has no page to show it on.
                return;
            }

            this.ShowPlayer(playerIndex, OwnerTabs.Mines);
            this.players[playerIndex].MapObjects.PickMine(mineIndex);
        }

        /// <summary>
        /// Shows the tab of the dwellings of the owner of the dwelling at <paramref name="dwellingIndex"/>, or of Neutral.
        /// </summary>
        public void ShowDwelling(int dwellingIndex)
        {
            var game = this.Loaded();
            var owner = game.History.Current.Dwellings().GetModel(dwellingIndex).Owner;
            if (owner.IsNeutral)
            {
                this.ShowNeutral(NeutralTabs.Dwellings);
                return;
            }

            var playerIndex = game.History.Current.Players().FindByColor(owner);
            if (playerIndex < 0)
            {
                // TODO:LOG Warning: a dwelling owned by a colour that has no player in the savegame has no page to show it on.
                return;
            }

            this.ShowPlayer(playerIndex, OwnerTabs.Dwellings);
        }

        /// <summary>
        /// Shows the page of the castle at <paramref name="castleIndex"/> in the savegame's castles.
        /// </summary>
        public void ShowCastle(int castleIndex)
        {
            var game = this.Loaded();

            if (!this.castles.TryGetValue(castleIndex, out var castle))
            {
                castle = new CastleViewModel(game.History, castleIndex, this.byteEditTargets, this.castleBuildMessages, this.ShowHero);
                this.castles[castleIndex] = castle;
            }

            this.SetCurrentPage(castle);
        }

        /// <inheritdoc/>
        protected override object? PrepareGame(Game game) =>
            new OverviewViewModel(game, this.banishingText, this.ShowHero, index => this.ShowPlayer(index), this.ShowCastle);

        /// <inheritdoc/>
        protected override void CloseGame()
        {
            foreach (var page in this.players.Values.Cast<IDisposable>().Concat(this.heroes.Values).Concat(this.castles.Values))
            {
                page.Dispose();
            }

            this.players.Clear();
            this.heroes.Clear();
            this.castles.Clear();
            this.map?.Dispose();
            this.map = null;
            this.neutral?.Dispose();
            this.neutral = null;
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
            NeutralViewModel neutralPage => this.Navigation.NeutralTabNode(neutralPage.SelectedTab),
            MapViewModel => this.Navigation.MapNode,
            _ => null,
        };

        /// <inheritdoc/>
        protected override void OnNodeSelected(NavNodeViewModel node)
        {
            switch (node.Kind)
            {
                case NavNodeKind.Hero:
                    this.ShowHero(node.Index);
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
                case NavNodeKind.OwnerSubGroup when node.Index < 0:
                    this.ShowNeutral(node.Tab);
                    break;
                case NavNodeKind.OwnerSubGroup:
                    this.ShowPlayer(node.Index, node.Tab);
                    break;
                case NavNodeKind.Neutral:
                case NavNodeKind.NeutralTab:
                    this.ShowNeutral(node.Tab);
                    break;
                case NavNodeKind.Mine:
                    this.ShowMine(node.Index);
                    break;
                case NavNodeKind.Dwelling:
                    this.ShowDwelling(node.Index);
                    break;
                case NavNodeKind.Overview:
                    this.ShowOverview();
                    break;
                case NavNodeKind.Map:
                    this.ShowMap();
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
    }
}
