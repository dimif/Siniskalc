namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
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
        private readonly CreatureBankTextProvider bankText;
        private readonly BanishingTextProvider banishingText;
        private readonly CastleBuildMessageProvider castleBuildMessages;
        private readonly ISpellPointsCalculatorFactory spellPoints;
        private readonly IByteEditTargetFactory byteEditTargets;
        private readonly Dictionary<int, PlayerViewModel> players = [];
        private readonly Dictionary<int, HeroViewModel> heroes = [];
        private readonly Dictionary<int, CastleViewModel> castles = [];
        private readonly Dictionary<PlayerColor, NeutralViewModel> neutrals = [];
        private OverviewViewModel? overview;

        private MapViewModel? map;

        /// <summary>
        /// Initializes a new instance of the <see cref="Homm3EditorViewModel"/> class.
        /// </summary>
        /// <param name="gameService">The game service.</param>
        /// <param name="effectLines">The effect lines.</param>
        /// <param name="banishingText">The banishing text.</param>
        /// <param name="castleBuildMessages">The castle build messages.</param>
        /// <param name="spellPoints">The spell points.</param>
        /// <param name="saveRegions">The save regions.</param>
        /// <param name="byteEditTargets">The byte edit targets.</param>
        /// <param name="navigationBuilder">The navigation builder.</param>
        public Homm3EditorViewModel(ISavegameService<Game> gameService, EffectLineProvider effectLines, BanishingTextProvider banishingText, CastleBuildMessageProvider castleBuildMessages, ISpellPointsCalculatorFactory spellPoints, ISaveRegionProvider saveRegions, IByteEditTargetFactory byteEditTargets, INavigationBuilder<Game> navigationBuilder, CreatureBankTextProvider bankText)
            : base(gameService, saveRegions, navigationBuilder)
        {
            this.effectLines = effectLines;
            this.bankText = bankText;
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
        /// <param name="heroIndex">The number of the hero.</param>
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
        /// <param name="heroIndex">The number of the hero.</param>
        public void ShowHero(int heroIndex)
        {
            var game = this.Loaded();

            if (!this.heroes.TryGetValue(heroIndex, out var hero))
            {
                hero = new HeroViewModel(game.History.CreateHeroSession(heroIndex), this.effectLines, this.spellPoints.Create(game.History.Current.GameVersion()), this.byteEditTargets, game.Places, game.Map?.HasUnderground ?? true, this.ShowCastle);
                this.heroes[heroIndex] = hero;
            }

            this.SetCurrentPage(hero);
        }

        /// <summary>
        /// Shows the page of the player at <paramref name="playerIndex"/> in the savegame's players.
        /// </summary>
        /// <param name="playerIndex">The place of the player's record, from 0.</param>
        /// <param name="tab">The tab to show.</param>
        public void ShowPlayer(int playerIndex, int tab = 0)
        {
            var game = this.Loaded();

            if (!this.players.TryGetValue(playerIndex, out var player))
            {
                player = new PlayerViewModel(
                    game.History.CreatePlayerSession(playerIndex),
                    this.byteEditTargets,
                    this.banishingText,
                    game,
                    this.ShowHero,
                    this.ShowCastle);
                this.players[playerIndex] = player;
            }

            player.SelectedTab = tab;

            this.SetCurrentPage(player);
        }

        /// <summary>
        /// Shows the page of what nobody owns, on the tab <paramref name="tab"/> (see <see cref="NeutralTabs"/>), or of what a colour owns
        /// that has no player in the savegame, when <paramref name="owner"/> is given.
        /// </summary>
        /// <param name="tab">The tab to show.</param>
        /// <param name="owner">The colour whose page it is; <see langword="null"/> for what nobody owns.</param>
        public void ShowNeutral(int tab = NeutralTabs.Heroes, PlayerColor? owner = null)
        {
            var game = this.Loaded();
            var color = owner ?? PlayerColor.Neutral;
            if (!this.neutrals.TryGetValue(color, out var page))
            {
                page = new NeutralViewModel(game.History, this.byteEditTargets, this.ShowHero, this.ShowCastle, color);
                this.neutrals[color] = page;
            }

            page.SelectedTab = tab;
            this.SetCurrentPage(page);
        }

        /// <summary>
        /// Shows the page of what the map keeps that no player owns.
        /// </summary>
        public void ShowMap(int section = MapSections.Overview, int? bankIndex = null)
        {
            this.map ??= new MapViewModel(this.Loaded(), this.effectLines, this.bankText, this.byteEditTargets);
            this.map.ShowSection(section, bankIndex);
            this.SetCurrentPage(this.map);
        }

        /// <summary>
        /// Shows the mine at <paramref name="mineIndex"/> among the mines of the savegame: on the Mines tab of its owner, or of Neutral.
        /// </summary>
        /// <param name="mineIndex">The place of the mine in the list of mines.</param>
        public void ShowMine(int mineIndex)
        {
            var game = this.Loaded();
            var owner = game.History.Current.Mines().GetModel(mineIndex).Owner;
            var playerIndex = owner.IsNeutral ? -1 : game.History.Current.Players().FindByColor(owner);
            if (playerIndex < 0)
            {
                // A colour with no player in the savegame has a page like Neutral's for what it owns.
                this.ShowNeutral(NeutralTabs.Mines, owner);
                this.neutrals[owner].MapObjects.PickMine(mineIndex);
                return;
            }

            this.ShowPlayer(playerIndex, OwnerTabs.Mines);
            this.players[playerIndex].MapObjects.PickMine(mineIndex);
        }

        /// <summary>
        /// Shows the dwelling at <paramref name="dwellingIndex"/> among the dwellings of the savegame: on the Dwellings tab of its owner, or of Neutral.
        /// </summary>
        /// <param name="dwellingIndex">The place of the dwelling in the list of dwellings.</param>
        public void ShowDwelling(int dwellingIndex)
        {
            var game = this.Loaded();
            var owner = game.History.Current.Dwellings().GetModel(dwellingIndex).Owner;
            var playerIndex = owner.IsNeutral ? -1 : game.History.Current.Players().FindByColor(owner);
            if (playerIndex < 0)
            {
                this.ShowNeutral(NeutralTabs.Dwellings, owner);
                this.neutrals[owner].MapObjects.PickDwelling(dwellingIndex);
                return;
            }

            this.ShowPlayer(playerIndex, OwnerTabs.Dwellings);
            this.players[playerIndex].MapObjects.PickDwelling(dwellingIndex);
        }

        /// <summary>
        /// Shows the page of the castle at <paramref name="castleIndex"/> in the savegame's castles.
        /// </summary>
        /// <param name="castleIndex">The number of the castle.</param>
        public void ShowCastle(int castleIndex)
        {
            var game = this.Loaded();

            if (!this.castles.TryGetValue(castleIndex, out var castle))
            {
                castle = new CastleViewModel(game.History.CreateCastleSession(castleIndex), this.byteEditTargets, this.castleBuildMessages, this.effectLines, this.ShowHero);
                this.castles[castleIndex] = castle;
            }

            this.SetCurrentPage(castle);
        }

        /// <inheritdoc/>
        public override AbstractFinderViewModel CreateArtifactCatalog() =>
            ArtifactFinderViewModel.ForCatalog(this.Loaded().History.Current.Catalog(), this.effectLines);

        /// <inheritdoc/>
        public override AbstractFinderViewModel CreateCreatureCatalog() =>
            new CreatureFinderViewModel(this.Loaded().History.Current.Catalog().Creatures.All.Select(c => ((Heroes.Entities.ICreature)c, c.Info)));

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
            foreach (var page in this.neutrals.Values)
            {
                page.Dispose();
            }

            this.neutrals.Clear();
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
            NeutralViewModel neutralPage => this.Navigation.NeutralTabNode(neutralPage.SelectedTab, neutralPage.OwnerIndex),
            MapViewModel map => this.MapNodeOf(map),
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
                    this.ShowNeutral(node.Tab, NeutralViewModel.OwnerOf(node.Index));
                    break;
                case NavNodeKind.OwnerSubGroup:
                    this.ShowPlayer(node.Index, node.Tab);
                    break;
                case NavNodeKind.Neutral:
                case NavNodeKind.NeutralTab:
                    this.ShowNeutral(node.Tab, NeutralViewModel.OwnerOf(node.Index));
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
                case NavNodeKind.MapSection:
                    this.ShowMap(node.Tab);
                    break;
                case NavNodeKind.MapObject:
                    this.ShowMap(node.Tab, node.Index);
                    break;
            }
        }

        private NavNodeViewModel? MapNodeOf(MapViewModel page)
        {
            var mapNode = this.Navigation.MapNode;
            if (page.SelectedTab is MapSections.Overview or MapSections.Hex || mapNode == null)
            {
                return mapNode;
            }

            var section = mapNode.Children.Concat(mapNode.Children.SelectMany(n => n.Children))
                .FirstOrDefault(n => n.Kind == NavNodeKind.MapSection && n.Tab == (page.SelectedTab == MapSections.BankDetails ? page.BankSection : page.SelectedTab));
            return page.SelectedBank is { } selected
                ? section?.Children.FirstOrDefault(n => n.Index == selected.Index) ?? section
                : section;
        }
    }
}
