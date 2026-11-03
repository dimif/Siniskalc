namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Runtime;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// ViewModel that wraps a player for UI binding. Like <see cref="HeroViewModel"/>, it reads from a
    /// <see cref="PlayerEditSession"/>, writes every change back through it, and refreshes when the history changes.
    /// </summary>
    public sealed class PlayerViewModel : SessionPageViewModelBase<PlayerModel>, IPlayerPage
    {
        private static readonly ResourceLimits Limits = new(
            Constants.Resources.Min,
            Constants.Resources.MaxGold,
            Constants.Resources.MaxOther,
            Constants.Resources.GoldStep,
            Constants.Resources.OtherStep,
            Constants.Resources.AllResourcesIncrease,
            Constants.Resources.GoldIncrease,
            Constants.Resources.OtherPreset,
            Constants.Resources.GoldPreset);

        private readonly ResourcesEditorViewModel resources;
        private readonly PlayerEditSession session;
        private readonly IGameInfo<PlayerColor> game;
        private readonly IByteEditTargetFactory byteEditTargets;
        private readonly BanishingTextProvider banishingText;
        private readonly Action<int>? openHero;
        private readonly Action<int>? openCastle;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerViewModel"/> class.
        /// </summary>
        /// <param name="session">The session that edits the player.</param>
        /// <param name="byteEditTargets">Makes the targets of the hex editor.</param>
        /// <param name="banishingText">Words the countdown of a player who is losing or gaining a town.</param>
        /// <param name="game">The live control and current-turn information of the game.</param>
        /// <param name="openHero">Opens the page of a hero, by its number; <see langword="null"/> for none.</param>
        /// <param name="openCastle">Opens the page of a castle, by its number; <see langword="null"/> for none.</param>
        public PlayerViewModel(PlayerEditSession session, IByteEditTargetFactory byteEditTargets, BanishingTextProvider banishingText, IGameInfo<PlayerColor> game, Action<int>? openHero = null, Action<int>? openCastle = null)
            : base(session.Current.Color.ToString(), session)
        {
            this.session = session;
            this.game = game;
            this.byteEditTargets = byteEditTargets;
            this.banishingText = banishingText;
            this.openHero = openHero;
            this.openCastle = openCastle;
            this.resources = ResourcesEditorViewModel.ForPlayer(
                Limits,
                () => this.session.Current.Resources,
                () => this.Session.Original.Resources,
                edit => this.session.Update(player => player with { Resources = edit(player.Resources) }),
                this.IncomeNote);
            this.DaysLeftCard = new DaysLeftCardViewModel(
                () => this.session.Current.DaysLeft,
                days => this.session.Update(player => player.WithDaysLeft(days)),
                () => this.session.Current.IsBanished);
            this.Obelisks = new ObeliskVisitsViewModel<PlayerColor>(session.History, Homm3ChunkKinds.Obelisks, session.Current.Color, "Grail", hasLevels: true);
            this.MapObjects = new OwnedMapObjectsViewModel(session.History, session.Current.Color, byteEditTargets);
            this.TavernPlaces = TavernPlaceViewModel.ForTavern(
                () => this.session.Current.Tavern,
                edit => this.session.Update(player => player with { Tavern = edit(player.Tavern) }),
                () => this.session.History.Current.Heroes(),
                this.openHero);

            this.UpdateIsDirty();
            this.RefreshOwned();
        }

        /// <summary>
        /// Gets the session that edits the player.
        /// </summary>
        public new PlayerEditSession Session => this.session;

        /// <summary>
        /// Gets the colour of the player.
        /// </summary>
        public PlayerColor Color => this.session.Current.Color;

        /// <inheritdoc/>
        public string Name => this.Color.ToString();

        /// <summary>
        /// Gets who plays the colour, followed by "current turn" when it is the turn of the player.
        /// </summary>
        public string ControlText => PlayerTypeText.Describe(this.game, this.Color);

        /// <inheritdoc/>
        public string ColorHex => PlayerPalette.Of(this.Color);

        /// <summary>
        /// Gets the resources, gold first (the game lists it last, but it is what players look for first).
        /// </summary>
        public IReadOnlyList<ResourceRowViewModel> Resources => this.resources.Rows;

        /// <summary>
        /// Gets the heroes of the player, by name.
        /// </summary>
        public IReadOnlyList<HeroLinkViewModel> Heroes { get; private set; } = [];

        /// <inheritdoc/>
        public bool HasHeroes => this.Heroes.Count > 0;

        /// <summary>
        /// Gets the two heroes the tavern of the player offers, each with the heroes that can take its place.
        /// </summary>
        public IReadOnlyList<TavernPlaceViewModel> TavernPlaces { get; }

        /// <summary>
        /// Gets the mines and dwellings of the map that the player owns, for the Mines and Dwellings tabs.
        /// </summary>
        public OwnedMapObjectsViewModel MapObjects { get; }

        /// <summary>
        /// Gets the obelisks of the map and which of them this player has visited; empty when the savefile has none to show.
        /// </summary>
        public ObeliskVisitsViewModel<PlayerColor> Obelisks { get; }

        /// <summary>
        /// Gets the towns of the player, by name.
        /// </summary>
        public IReadOnlyList<CastleLinkViewModel> Castles { get; private set; } = [];

        /// <inheritdoc/>
        public bool HasCastles => this.Castles.Count > 0;

        /// <summary>
        /// Gets the days left before the player is banished, or null while the player owns a town.
        /// </summary>
        public int? DaysLeft => this.session.Current.DaysLeft;

        /// <summary>
        /// Gets whether the player owns no town, which the pill beside its name shows.
        /// </summary>
        public bool HasBanishing => this.DaysLeft != null;

        /// <summary>
        /// Gets whether the player is out of the game.
        /// </summary>
        public bool IsBanished => this.session.Current.IsBanished;

        /// <summary>
        /// Gets the pill beside the name of a player who owns no town, such as "No towns: 6 days left" or "Banished".
        /// </summary>
        public string BanishingText => this.banishingText.Pill(this.session.Current);

        /// <summary>
        /// Gets the Banishing card: the days left of a player who owns no town, which can be changed while it is not banished.
        /// </summary>
        public DaysLeftCardViewModel DaysLeftCard { get; }

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
        /// Creates the raw bytes of the player for the hex editor. The caller owns the target and disposes it.
        /// </summary>
        /// <returns>The target.</returns>
        public IByteEditTarget CreateByteEditTarget() => this.byteEditTargets.ForPlayer(this.session);

        /// <summary>
        /// Gives the player <paramref name="amount"/> more of each resource of the rows, up to the most a resource can hold.
        /// </summary>
        /// <inheritdoc/>
        protected override void OnHistoryChanged()
        {
            this.RaisePropertyChanged(nameof(this.ControlText));
            this.resources.Refresh();

            this.UpdateIsDirty();
            this.RefreshOwned();
            this.RefreshBanishing();
            this.Obelisks.Refresh();
            this.MapObjects.Refresh();
            foreach (var place in this.TavernPlaces)
            {
                place.Refresh();
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void RefreshOwned()
        {
            var current = this.session.History.Current;
            this.Heroes = OwnedLinks.Heroes(current.Heroes(), this.Color, this.openHero);
            this.Castles = CastleLinks.OwnedBy(current, this.Color, this.openCastle, this.openHero);
            this.RaisePropertyChanged(nameof(this.Heroes));
            this.RaisePropertyChanged(nameof(this.HasHeroes));
            this.RaisePropertyChanged(nameof(this.Castles));
            this.RaisePropertyChanged(nameof(this.HasCastles));
        }

        // The savegame keeps no income: it is worked out from the mines and the town halls the player owns, so it is what the player earns at least.
        private string IncomeNote(Resource resource)
        {
            var document = this.session.History.Current;
            var income = DailyIncome.Of(this.session.Current.Color, document.Mines().GetModels(), document.Castles().GetModels());
            return income[resource] > 0 ? $"+{income[resource]:N0} a day" : string.Empty;
        }

        private void RefreshBanishing()
        {
            this.RaisePropertyChanged(nameof(this.DaysLeft));
            this.RaisePropertyChanged(nameof(this.HasBanishing));
            this.RaisePropertyChanged(nameof(this.IsBanished));
            this.RaisePropertyChanged(nameof(this.BanishingText));
            this.DaysLeftCard.Refresh();
        }
    }
}
