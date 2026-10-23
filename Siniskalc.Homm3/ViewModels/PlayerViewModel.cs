namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

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
            Constants.Resources.GoldIncrease);

        private readonly ResourcesEditorViewModel resources;
        private readonly PlayerEditSession session;
        private readonly IByteEditTargetFactory byteEditTargets;
        private readonly BanishingTextProvider banishingText;
        private readonly Action<int>? openHero;
        private readonly Action<int>? openCastle;

        public PlayerViewModel(PlayerEditSession session, IByteEditTargetFactory byteEditTargets, BanishingTextProvider banishingText, PlayerType? control = null, bool isCurrent = false, Action<int>? openHero = null, Action<int>? openCastle = null)
            : base(session.Current.Color.ToString(), session)
        {
            this.session = session;
            this.byteEditTargets = byteEditTargets;
            this.banishingText = banishingText;
            this.openHero = openHero;
            this.openCastle = openCastle;
            this.ControlText = PlayerTypeText.Describe(control, isCurrent);
            this.resources = new ResourcesEditorViewModel(
                ResourceSet.All.OrderBy(resource => resource != Resource.Gold),
                Limits,
                () => this.session.Current.Resources,
                () => this.Session.Original.Resources,
                edit => this.session.Update(player => player with { Resources = edit(player.Resources) }));
            this.DaysLeftCard = new DaysLeftCardViewModel(
                () => this.session.Current.DaysLeft,
                days => this.session.Update(player => player.WithDaysLeft(days)),
                () => this.session.Current.IsBanished);
            this.Obelisks = new ObeliskVisitsViewModel<PlayerColor>(session.History, Homm3ChunkKinds.Obelisks, session.Current.Color, "Grail", at => MapLevelText.Coordinates(at.X, at.Y, at.Level));
            this.MapObjects = new OwnedMapObjectsViewModel(session.History, session.Current.Color, byteEditTargets);
            this.TavernPlaces = TavernPlaceViewModel.ForTavern(
                () => this.session.Current.Tavern,
                edit => this.session.Update(player => player with { Tavern = edit(player.Tavern) }),
                () => this.session.History.Current.Heroes(),
                this.OpenHeroById);

            this.UpdateIsDirty();
            this.RefreshOwned();
        }

        public new PlayerEditSession Session => this.session;

        public PlayerColor Color => this.session.Current.Color;

        /// <inheritdoc/>
        public string Name => this.Color.ToString();

        /// <summary>
        /// Gets "Human" or "Computer", or nothing when the savefile does not tell.
        /// </summary>
        public string ControlText { get; }

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
        IReadOnlyList<AmountRowViewModel> IResourcesPage.Resources => this.Resources;

        /// <inheritdoc/>
        IReadOnlyList<ILink> IPlayerPage.Heroes => this.Heroes;

        /// <inheritdoc/>
        IReadOnlyList<ILink> IPlayerPage.Castles => this.Castles;

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
        /// Gets the player as the savefile had it when it was loaded or last saved.
        /// </summary>
        /// <summary>
        /// Creates the raw bytes of the player for the hex editor. The caller owns the target and disposes it.
        /// </summary>
        public IByteEditTarget CreateByteEditTarget() => this.byteEditTargets.ForPlayer(this.session);

        /// <summary>
        /// Gives the player <paramref name="amount"/> more of each resource of the rows, up to the most a resource can hold.
        /// </summary>
        /// <inheritdoc/>
        protected override void OnHistoryChanged()
        {
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

        private void OpenHeroById(int heroId)
        {
            if (this.session.History.Current.Heroes().FindById(heroId) is { } hero)
            {
                this.openHero?.Invoke(hero.Index);
            }
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
