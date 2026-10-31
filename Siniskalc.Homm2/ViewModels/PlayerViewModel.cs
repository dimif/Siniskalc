namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;

    /// <summary>
    /// The pages of a player: the resources and what they earn each day, the heroes and the tavern, and the towns.
    /// </summary>
    public sealed class PlayerViewModel : SessionPageViewModelBase<PlayerModel>, IPlayerPage, IOwnedMines
    {
        private static readonly ResourceLimits Limits = new(
            Constants.Player.MinResource,
            Constants.Player.MaxGold,
            Constants.Player.MaxOther,
            Constants.Player.GoldStep,
            Constants.Player.OtherStep,
            Constants.Player.AllResourcesIncrease,
            Constants.Player.GoldIncrease,
            Constants.Player.OtherPreset,
            Constants.Player.GoldPreset);

        private readonly BanishingTextProvider banishingText;
        private readonly Action<int> openHero;
        private readonly Action<int> openCastle;
        private readonly ResourcesEditorViewModel resources;
        private readonly PlayerEditSession session;
        private readonly IByteEditTargetFactory? byteEditTargets;
        private IReadOnlyList<HeroLinkViewModel> heroes = [];
        private IReadOnlyList<CastleLinkViewModel> castles = [];
        private string banishing = string.Empty;

        public PlayerViewModel(Game game, PlayerColor color, BanishingTextProvider banishingText, Action<int> openHero, Action<int> openCastle, IByteEditTargetFactory? byteEditTargets = null)
            : this(game, color, game.History.CreatePlayerSession(color.Code), banishingText, openHero, openCastle, byteEditTargets)
        {
        }

        private PlayerViewModel(Game game, PlayerColor color, PlayerEditSession session, BanishingTextProvider banishingText, Action<int> openHero, Action<int> openCastle, IByteEditTargetFactory? byteEditTargets)
            : base("Player", session)
        {
            this.byteEditTargets = byteEditTargets;
            this.Game = game;
            this.Color = color;
            this.banishingText = banishingText;
            this.openHero = openHero;
            this.openCastle = openCastle;
            this.session = session;
            this.Mines = MinesLists.Create(game, color);
            this.Obelisks = new ObeliskVisitsViewModel<PlayerColor>(game.History, Homm2ChunkKinds.Obelisks, color, "Ultimate Artifact", hasLevels: false);
            this.resources = ResourcesEditorViewModel.ForPlayer(
                Limits,
                () => this.session.Current.Resources,
                () => this.Game.History.Original.Players().GetModel(this.Color.Code).Resources,
                edit => this.session.Update(player => player with { Resources = edit(player.Resources) }),
                this.IncomeNote);
            this.TavernPlaces = TavernPlaceViewModel.ForTavern(
                () => this.session.Current.Tavern,
                edit => this.session.Update(player => player with { Tavern = edit(player.Tavern) }),
                () => this.Game.History.Current.Heroes(),
                this.openHero);
            this.DaysLeftCard = new DaysLeftCardViewModel(
                () => this.session.Current.DaysLeft,
                days => this.session.Update(player => player.WithDaysLeft(days)),
                () => this.session.Current.IsBanished);
            this.Refresh();
        }

        public PlayerColor Color { get; }

        /// <inheritdoc/>
        public string Name => this.Color.ToString();

        /// <summary>
        /// Gets who plays the colour ("Human" or "Computer"), followed by "current turn" when it is the turn of the player.
        /// </summary>
        public string ControlText => PlayerTypeText.Describe(this.Game, this.Color);

        /// <inheritdoc/>
        public string ColorHex => PlayerPalette.Of(this.Color);

        /// <summary>
        /// Gets the pill of a player who owns no town, such as "No towns: 6 days left" or "Banished", or nothing for a player who owns one.
        /// </summary>
        public string BanishingText => this.banishing;

        /// <summary>
        /// Gets the Banishing card: the days left of a player who owns no town, which can be changed while it is not banished.
        /// </summary>
        public DaysLeftCardViewModel DaysLeftCard { get; }

        /// <inheritdoc/>
        public IReadOnlyList<ResourceRowViewModel> Resources => this.resources.Rows;

        /// <inheritdoc/>
        public ICommand SetAllResourcesCommand => this.resources.SetAllCommand;

        /// <inheritdoc/>
        public ICommand SetRichCommand => this.resources.SetRichCommand;

        /// <summary>
        /// Gets the command that sets every resource but gold to 0.
        /// </summary>
        public ICommand ClearAllResourcesCommand => this.resources.ClearAllCommand;

        /// <summary>
        /// Gets the command that sets gold to 0.
        /// </summary>
        public ICommand ClearRichCommand => this.resources.ClearRichCommand;

        /// <inheritdoc/>
        public string SetAllLabel => this.resources.SetAllLabel;

        /// <inheritdoc/>
        public string SetRichLabel => this.resources.SetRichLabel;

        /// <summary>
        /// Gets the mines of the map that the player owns, for the Mines tab.
        /// </summary>
        public MinesListViewModel<MineRowViewModel<MineModel, PlayerColor>> Mines { get; }

        /// <summary>
        /// Gets the obelisks of the map and which of them this player has visited.
        /// </summary>
        public ObeliskVisitsViewModel<PlayerColor> Obelisks { get; }

        /// <inheritdoc/>
        public IReadOnlyList<HeroLinkViewModel> Heroes => this.heroes;

        /// <inheritdoc/>
        public IReadOnlyList<CastleLinkViewModel> Castles => this.castles;

        /// <summary>
        /// Gets the two places of the tavern of the player, each with the heroes it can be given to.
        /// </summary>
        public IReadOnlyList<TavernPlaceViewModel> TavernPlaces { get; }

        /// <inheritdoc/>
        public bool HasHeroes => this.heroes.Count > 0;

        /// <inheritdoc/>
        public bool HasCastles => this.castles.Count > 0;

        private Game Game { get; }

        /// <summary>
        /// Creates the target the hex tab of the page works on: the raw bytes of the player. The caller disposes it. <c>null</c> when the
        /// editor was built without the means to make one, and the page has no hex tab.
        /// </summary>
        public Siniskalc.Core.Data.IByteEditTarget? CreateByteEditTarget() => this.byteEditTargets?.ForPlayer(this.session);

        /// <inheritdoc/>
        public Siniskalc.Core.Data.IByteEditTarget? CreateMineByteEditTarget() =>
            this.Mines.SelectedMine is { } mine ? this.byteEditTargets?.ForMine(this.session.History, mine.Index) : null;

        /// <summary>
        /// Reads what the page shows from the current document. Called by the page once it is built, and after every change.
        /// </summary>
        public void Refresh()
        {
            this.Read();
            this.UpdateIsDirty();
            this.RaiseAllPropertiesChanged();
        }

        /// <inheritdoc/>
        protected override void OnHistoryChanged() => this.Refresh();

        private void Read()
        {
            var document = this.Game.History.Current;
            var player = document.Players().GetModel(this.Color.Code);
            this.banishing = this.banishingText.Pill(player);

            this.resources.Refresh();

            this.heroes = OwnedLinks.Heroes(document.Heroes(), this.Color, this.openHero);
            this.castles = CastleLinks.OwnedBy(document, this.Color, this.openCastle, this.openHero);

            this.Obelisks?.Refresh();
            this.Mines?.Refresh();
            this.DaysLeftCard?.Refresh();

            foreach (var place in this.TavernPlaces)
            {
                place.Refresh();
            }
        }

        // The game keeps no income for a player that was put out: what the table holds is a counter that walks over the resources
        // (1 wood, then 1 mercury, then 1 ore, on the days after Green was put out in the Broken Alliance saves 22 to 24).
        private string IncomeNote(Resource resource)
        {
            var player = this.session.Current;
            return !player.IsBanished && player.Income[resource] > 0 ? $"+{player.Income[resource]:N0} a day" : string.Empty;
        }
    }
}
