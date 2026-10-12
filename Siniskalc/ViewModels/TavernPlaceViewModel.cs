namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One of the two places of a player's tavern: who is in it, and the heroes it can be given to.
    /// </summary>
    public sealed class TavernPlaceViewModel : AbstractViewModelBase
    {
        private readonly Func<IReadOnlyList<HeroChoiceViewModel>> getChoices;
        private readonly Func<int?> getHeroId;
        private readonly Action<int?> setHeroId;
        private IReadOnlyList<HeroChoiceViewModel> choices = [];

        /// <param name="label">What the place is called, such as "First hero".</param>
        /// <param name="getChoices">Gives the heroes to pick from, with nobody first, as the savefile stands now.</param>
        /// <param name="getHeroId">Gives the id of the hero that is in the place, or null.</param>
        /// <param name="setHeroId">Puts the hero with this id (or nobody) in the place.</param>
        /// <param name="openHero">Opens the page of the hero with this id; the link does nothing without it or for a place that is empty.</param>
        public TavernPlaceViewModel(string label, Func<IReadOnlyList<HeroChoiceViewModel>> getChoices, Func<int?> getHeroId, Action<int?> setHeroId, Action<int>? openHero = null)
        {
            this.OpenCommand = new DelegateCommand(_ => openHero?.Invoke(this.getHeroId()!.Value), _ => openHero != null && this.getHeroId() != null);
            this.Label = label;
            this.getChoices = getChoices;
            this.getHeroId = getHeroId;
            this.setHeroId = setHeroId;
            this.Refresh();
        }

        public string Label { get; }

        /// <summary>
        /// Gets the command that opens the page of the hero in the place.
        /// </summary>
        public ICommand OpenCommand { get; }

        /// <summary>
        /// Gets the heroes the place can be given to: nobody, the hero in it now, and every hero that no player owns and that is not in the other place.
        /// </summary>
        public IReadOnlyList<HeroChoiceViewModel> Choices => this.choices;

        /// <summary>
        /// Gets or sets who is in the place.
        /// </summary>
        public HeroChoiceViewModel? Selected
        {
            get => this.choices.FirstOrDefault(c => c.HeroId == this.getHeroId());
            set
            {
                if (value != null && value.HeroId != this.getHeroId())
                {
                    this.setHeroId(value.HeroId);
                }
            }
        }

        /// <summary>
        /// Gets or sets the id of the hero in the place, <see cref="HeroChoiceViewModel.NoneKey"/> for nobody. The combo box selects by it, which survives its items being replaced.
        /// </summary>
        public int SelectedKey
        {
            get => this.getHeroId() ?? HeroChoiceViewModel.NoneKey;
            set
            {
                var heroId = value == HeroChoiceViewModel.NoneKey ? (int?)null : value;
                if (heroId != this.getHeroId())
                {
                    this.setHeroId(heroId);
                }
            }
        }

        /// <summary>
        /// Shows the place again, after an edit or an undo.
        /// </summary>
        public void Refresh()
        {
            var fresh = this.getChoices();
            if (!fresh.Select(c => (c.HeroId, c.Name)).SequenceEqual(this.choices.Select(c => (c.HeroId, c.Name))))
            {
                // A combo box drops its selection when its items are replaced, so the items are only replaced when they differ.
                this.choices = fresh;
                this.RaisePropertyChanged(nameof(this.Choices));
            }

            this.RaisePropertyChanged(nameof(this.Selected));
            this.RaisePropertyChanged(nameof(this.SelectedKey));
        }
    }
}
