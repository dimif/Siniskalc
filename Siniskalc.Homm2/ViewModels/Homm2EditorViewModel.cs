namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The editor of Heroes of Might and Magic II: the open savegame, its history and what it shows. The application shell
    /// (<see cref="ShellViewModel"/>) works with it through <see cref="IGameEditor"/>.
    /// </summary>
    public sealed class Homm2EditorViewModel : EditorViewModelBase<Game>
    {
        private readonly IByteEditTargetFactory byteEditTargets;
        private readonly BanishingTextProvider banishingText;
        private readonly IMaxSpellPointsCalculator<SecondarySkill, IHomm2SecondarySkills> spellPoints;
        private readonly Dictionary<int, PlayerViewModel> players = [];
        private readonly Dictionary<int, HeroViewModel> heroes = [];
        private readonly Dictionary<int, CastleViewModel> castles = [];
        private OverviewViewModel? overview;
        private MapViewModel? map;

        public Homm2EditorViewModel(IGameService gameService, ISaveRegionProvider saveRegions, IByteEditTargetFactory byteEditTargets, INavigationBuilder<Game> navigationBuilder, BanishingTextProvider banishingText, IMaxSpellPointsCalculator<SecondarySkill, IHomm2SecondarySkills> spellPoints)
            : base(gameService, saveRegions, navigationBuilder)
        {
            this.byteEditTargets = byteEditTargets;
            this.banishingText = banishingText;
            this.spellPoints = spellPoints;
        }

        /// <summary>
        /// Gets what the editor shows about the loaded game, or <c>null</c> when none is loaded.
        /// </summary>
        public OverviewViewModel? Overview => this.overview;

        /// <inheritdoc/>
        protected override object? OverviewPage => this.overview;

        /// <summary>
        /// Shows the page of the player of <paramref name="color"/>, on the tab <paramref name="tab"/>.
        /// </summary>
        public void ShowPlayer(PlayerColor color, int tab = 0)
        {
            var game = this.Loaded();
            if (!this.players.TryGetValue(color.Code, out var player))
            {
                player = new PlayerViewModel(game, color, this.ShowHero, this.ShowCastle, this.byteEditTargets);
                this.players[color.Code] = player;
            }

            player.SelectedTab = tab;
            this.SetCurrentPage(player);
        }

        /// <summary>
        /// Shows the page of the objects of the map: its mines, shrines and sphinxes.
        /// </summary>
        public void ShowMap()
        {
            this.map ??= new MapViewModel(this.Loaded());
            this.SetCurrentPage(this.map);
        }

        /// <summary>
        /// Shows the page of the hero with the number <paramref name="heroId"/>.
        /// </summary>
        public void ShowHero(int heroId)
        {
            var game = this.Loaded();
            if (!this.heroes.TryGetValue(heroId, out var hero))
            {
                hero = new HeroViewModel(game, heroId, this.ShowCastle, this.spellPoints, this.byteEditTargets);
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
                castle = new CastleViewModel(game, castleIndex, this.ShowHero, this.byteEditTargets);
                this.castles[castleIndex] = castle;
            }

            this.SetCurrentPage(castle);
        }

        /// <inheritdoc/>
        protected override object? PrepareGame(Game game) => new OverviewViewModel(game, this.banishingText, color => this.ShowPlayer(color), this.ShowHero, this.ShowCastle);

        /// <inheritdoc/>
        protected override void CloseGame()
        {
            foreach (var page in this.players.Values.Cast<PageViewModel>().Concat(this.heroes.Values).Concat(this.castles.Values))
            {
                page.Dispose();
            }

            this.players.Clear();
            this.heroes.Clear();
            this.castles.Clear();
            this.map?.Dispose();
            this.map = null;
            this.overview?.Dispose();
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
            PlayerViewModel player => this.Navigation.PlayerTabNode(player.Color.Code, player.SelectedTab),
            HeroViewModel hero => this.Navigation.HeroNode(hero.HeroId),
            CastleViewModel castle => this.Navigation.CastleNode(castle.CastleIndex),
            MapViewModel => this.Navigation.MapNode,
            _ => null,
        };

        /// <inheritdoc/>
        protected override void OnNodeSelected(NavNodeViewModel node)
        {
            switch (node.Kind)
            {
                case NavNodeKind.Overview:
                    this.ShowOverview();
                    break;
                case NavNodeKind.Player:
                    this.ShowPlayer(PlayerColor.All[node.Index]);
                    break;
                case NavNodeKind.PlayerTab:
                    this.ShowPlayer(PlayerColor.All[node.Index], node.Tab);
                    break;
                case NavNodeKind.Hero:
                    this.ShowHero(node.Index);
                    break;
                case NavNodeKind.Castle:
                    this.ShowCastle(node.Index);
                    break;
                case NavNodeKind.Map:
                    this.ShowMap();
                    break;
            }
        }
    }
}
