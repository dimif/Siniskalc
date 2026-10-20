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
    public sealed class PlayerViewModel : ResettablePageViewModelBase, IPlayerPage
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
            : base(session.Current.Color.ToString(), session.History)
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
                () => this.Original.Resources,
                edit => this.session.Update(player => player with { Resources = edit(player.Resources) }));
            this.DayChoices = Enumerable.Range(Constants.Player.MinDaysLeft + 1, Constants.Player.MaxDaysLeft - Constants.Player.MinDaysLeft)
                .Reverse()
                .Select(days => new DaysLeftChoiceViewModel(days, () => this.DaysLeft == days, () => this.session.Update(player => player.WithDaysLeft(days)), () => !this.IsBanished))
                .ToList();
            this.Obelisks = new ObeliskVisitsViewModel(session.History, session.Current.Color);
            this.TavernPlaces = Enumerable.Range(0, TavernHeroes.Places)
                .Select(place => new TavernPlaceViewModel(
                    place == 0 ? "First hero" : "Second hero",
                    () => TavernChoices.For(this.session.Current.Tavern, place, this.session.History.Current.Heroes().GetSummaries()),
                    () => this.session.Current.Tavern.Ids[place],
                    heroId => this.session.Update(player => player with { Tavern = player.Tavern.With(place, heroId) }),
                    heroId => this.OpenHeroById(heroId)))
                .ToList();

            this.UpdateIsDirty();
            this.RefreshOwned();
        }

        public PlayerEditSession Session => this.session;

        public PlayerColor Color => this.session.Current.Color;

        public string Name => this.Color.ToString();

        /// <summary>
        /// Gets "Human" or "Computer", or nothing when the savefile does not tell.
        /// </summary>
        public string ControlText { get; }

        public string ColorHex => PlayerPalette.Of(this.Color);

        /// <summary>
        /// Gets the resources, gold first (the game lists it last, but it is what players look for first).
        /// </summary>
        public IReadOnlyList<ResourceRowViewModel> Resources => this.resources.Rows;

        /// <summary>
        /// Gets the heroes of the player, by name.
        /// </summary>
        public IReadOnlyList<HeroLinkViewModel> Heroes { get; private set; } = [];

        IReadOnlyList<AmountRowViewModel> IResourcesPage.Resources => this.Resources;

        IReadOnlyList<ILink> IPlayerPage.Heroes => this.Heroes;

        IReadOnlyList<ILink> IPlayerPage.Castles => this.Castles;

        public bool HasHeroes => this.Heroes.Count > 0;

        /// <summary>
        /// Gets the two heroes the tavern of the player offers, each with the heroes that can take its place.
        /// </summary>
        public IReadOnlyList<TavernPlaceViewModel> TavernPlaces { get; }

        /// <summary>
        /// Gets the obelisks of the map and which of them this player has visited; empty when the savefile has none to show.
        /// </summary>
        public ObeliskVisitsViewModel Obelisks { get; }

        /// <summary>
        /// Gets the towns of the player, by name.
        /// </summary>
        public IReadOnlyList<CastleLinkViewModel> Castles { get; private set; } = [];

        public bool HasCastles => this.Castles.Count > 0;

        /// <summary>
        /// Gets the days left before the player is banished, or null while the player owns a town.
        /// </summary>
        public int? DaysLeft => this.session.Current.DaysLeft;

        /// <summary>
        /// Gets whether the Banishing card is shown: the player owns no town. A banished player is shown, but cannot be edited.
        /// </summary>
        public bool HasBanishing => this.DaysLeft != null;

        public bool IsBanished => this.session.Current.IsBanished;

        /// <summary>
        /// Gets the pill beside the name of a player who owns no town, such as "No towns: 6 days left" or "Banished".
        /// </summary>
        public string BanishingText => this.banishingText.Pill(this.session.Current);

        /// <summary>
        /// Gets the days the player can be given, 7 down to 1.
        /// </summary>
        public IReadOnlyList<DaysLeftChoiceViewModel> DayChoices { get; }

        public ICommand SetAllResourcesCommand => this.resources.SetAllCommand;

        public ICommand SetRichCommand => this.resources.SetRichCommand;


        /// <summary>
        /// Gets the player as the savefile had it when it was loaded or last saved.
        /// </summary>
        private PlayerModel Original => this.session.History.Original.Players().GetModel(this.session.PlayerIndex);

        /// <summary>
        /// Creates the raw bytes of the player for the hex editor. The caller owns the target and disposes it.
        /// </summary>
        public IByteEditTarget CreateByteEditTarget() => this.byteEditTargets.ForPlayer(this.session);

        /// <summary>
        /// Gives the player <paramref name="amount"/> more of each resource of the rows, up to the most a resource can hold.
        /// </summary>
        protected override void Reset() => this.session.Update(_ => this.Original);

        protected override void OnHistoryChanged()
        {
            this.resources.Refresh();

            this.UpdateIsDirty();
            this.RefreshOwned();
            this.RefreshBanishing();
            this.Obelisks.Refresh();
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
            this.Heroes = current.Heroes().GetSummaries()
                .Where(s => s.Owner == this.Color)
                .OrderBy(s => s.Name)
                .Select(s => new HeroLinkViewModel(s, new DelegateCommand(_ => this.openHero?.Invoke(s.Index), _ => this.openHero != null)))
                .ToList();
            this.Castles = current.Castles().GetModels()
                .Select((model, index) => (Index: index, Model: model))
                .Where(c => c.Model.Owner == this.Color)
                .OrderBy(c => c.Model.Name)
                .Select(c => CastleLinks.For(c.Model, new DelegateCommand(_ => this.openCastle?.Invoke(c.Index), _ => this.openCastle != null), CastleLinks.HeroesOf(c.Model, current.Heroes(), this.openHero)))
                .ToList();
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
            foreach (var choice in this.DayChoices)
            {
                choice.Refresh();
            }
        }

        private void UpdateIsDirty()
        {
            this.HasPendingChanges = this.session.IsDirty;
        }
    }
}
