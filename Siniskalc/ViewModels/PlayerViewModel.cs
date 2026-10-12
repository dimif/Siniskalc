namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Core.Data;
    using Siniskalc.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// ViewModel that wraps a player for UI binding. Like <see cref="HeroViewModel"/>, it reads from a
    /// <see cref="PlayerEditSession"/>, writes every change back through it, and refreshes when the history changes.
    /// </summary>
    public sealed class PlayerViewModel : PageViewModelBase, IDisposable
    {
        private readonly PlayerEditSession session;
        private readonly IByteEditTargetFactory byteEditTargets;
        private readonly Action<int>? openHero;
        private readonly Action<int>? openCastle;
        private int selectedTab;

        public PlayerViewModel(PlayerEditSession session, IByteEditTargetFactory byteEditTargets, PlayerType? control = null, bool isCurrent = false, Action<int>? openHero = null, Action<int>? openCastle = null)
            : base(session.Current.Color.ToString())
        {
            this.session = session;
            this.byteEditTargets = byteEditTargets;
            this.openHero = openHero;
            this.openCastle = openCastle;
            this.ControlText = PlayerTypeText.Describe(control, isCurrent);
            this.Resources = PlayerResources.All
                .OrderBy(resource => resource != Resource.Gold)
                .Select(resource => new ResourceRowViewModel(
                    resource,
                    () => this.session.Current.Resources,
                    () => this.Original.Resources,
                    edit => this.session.Update(player => player with { Resources = edit(player.Resources) })))
                .ToList();
            this.SetAllResourcesCommand = new DelegateCommand(_ => this.AddResources(this.Resources.Where(r => !r.IsGold), Constants.Resources.AllResourcesIncrease));
            this.SetRichCommand = new DelegateCommand(_ => this.AddResources(this.Resources.Where(r => r.IsGold), Constants.Resources.GoldIncrease));
            this.ResetCommand = new DelegateCommand(_ => this.session.Update(_ => this.Original), _ => this.HasPendingChanges);
            this.Obelisks = new ObeliskVisitsViewModel(session.History, session.Current.Color);
            this.TavernPlaces = Enumerable.Range(0, TavernHeroes.Places)
                .Select(place => new TavernPlaceViewModel(
                    place == 0 ? "First hero" : "Second hero",
                    () => this.TavernChoices(place),
                    () => this.session.Current.Tavern.Ids[place],
                    heroId => this.session.Update(player => player with { Tavern = player.Tavern.With(place, heroId) }),
                    heroId => this.OpenHeroById(heroId)))
                .ToList();

            this.session.History.Changed += this.OnHistoryChanged;
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
        public IReadOnlyList<ResourceRowViewModel> Resources { get; }

        /// <summary>
        /// Gets or sets the tab shown: 0 for the resources, 1 for the heroes, 2 for the towns, 3 for the obelisks, 4 for the hex editor.
        /// </summary>
        public int SelectedTab
        {
            get => this.selectedTab;
            set
            {
                if (this.selectedTab != value)
                {
                    this.selectedTab = value;
                    this.RaisePropertyChanged(nameof(this.SelectedTab));
                }
            }
        }

        /// <summary>
        /// Gets the heroes of the player, by name.
        /// </summary>
        public IReadOnlyList<HeroLinkViewModel> Heroes { get; private set; } = [];

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

        public ICommand SetAllResourcesCommand { get; }

        public ICommand SetRichCommand { get; }

        public ICommand ResetCommand { get; }

        /// <summary>
        /// Gets the player as the savefile had it when it was loaded or last saved.
        /// </summary>
        private PlayerModel Original => this.session.History.Original.Players().GetModel(this.session.PlayerIndex);

        /// <summary>
        /// Creates the raw bytes of the player for the hex editor. The caller owns the target and disposes it.
        /// </summary>
        public IByteEditTarget CreateByteEditTarget() => this.byteEditTargets.ForPlayer(this.session);

        public void Dispose()
        {
            this.session.History.Changed -= this.OnHistoryChanged;
        }

        /// <summary>
        /// Gives the player <paramref name="amount"/> more of each resource of the rows, up to the most a resource can hold.
        /// </summary>
        private void AddResources(IEnumerable<ResourceRowViewModel> rows, int amount)
        {
            var kinds = rows.Select(r => (r.Resource, r.Maximum)).ToList();
            this.session.Update(player => player with
            {
                Resources = kinds.Aggregate(player.Resources, (resources, kind) => resources.With(kind.Resource, (int)Math.Min(resources[kind.Resource] + (long)amount, kind.Maximum))),
            });
        }

        private void OnHistoryChanged(object? sender, EventArgs e)
        {
            foreach (var row in this.Resources)
            {
                row.Refresh();
            }

            this.UpdateIsDirty();
            this.RefreshOwned();
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

        /// <summary>
        /// Lists who can be put in a place of the tavern: nobody, the hero that is in it now, and the heroes no player owns, except the one in the other place.
        /// </summary>
        private IReadOnlyList<HeroChoiceViewModel> TavernChoices(int place)
        {
            var tavern = this.session.Current.Tavern;
            var inPlace = tavern.Ids[place];
            var inOther = tavern.Ids[TavernHeroes.Places - 1 - place];
            var candidates = this.session.History.Current.Heroes().GetSummaries()
                .Where(h => h.HeroId == inPlace || (h.Owner == null && h.HeroId != inOther))
                .OrderBy(h => h.Name, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(h => h.HeroId)
                .ToList();

            // Names are not unique in a savefile (there are two Lord Haarts, say), so a repeated name is told apart by the id.
            var repeated = candidates.GroupBy(h => h.Name).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();
            var choices = new List<HeroChoiceViewModel> { new(null, "(none)") };
            choices.AddRange(candidates.Select(h => new HeroChoiceViewModel(h.HeroId, repeated.Contains(h.Name) ? $"{h.Name} (id {h.HeroId})" : h.Name)));
            if (inPlace is { } id && candidates.All(h => h.HeroId != id))
            {
                // The place holds an id that the savefile has no hero for.
                choices.Add(new HeroChoiceViewModel(id, $"Unknown hero (id {id})"));
            }

            return choices;
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
                .Select(c => new CastleLinkViewModel(c.Model, new DelegateCommand(_ => this.openCastle?.Invoke(c.Index), _ => this.openCastle != null), CastleHeroRowViewModel.For(c.Model, current.Heroes(), this.openHero)))
                .ToList();
            this.RaisePropertyChanged(nameof(this.Heroes));
            this.RaisePropertyChanged(nameof(this.HasHeroes));
            this.RaisePropertyChanged(nameof(this.Castles));
            this.RaisePropertyChanged(nameof(this.HasCastles));
        }

        private void UpdateIsDirty()
        {
            this.HasPendingChanges = this.session.IsDirty;
        }
    }
}
