namespace Siniskalc.Homm2.ViewModels
{
    using System.Windows.Input;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One of the six dwellings of a castle: how far it is built (not built, built, upgraded) and how many creatures wait in it.
    /// </summary>
    public sealed class DwellingRowViewModel : AbstractViewModelBase, IStockedDwelling
    {
        private readonly System.Func<bool> isBuilt;
        private readonly System.Func<int> weeklyGrowth;

        /// <summary>
        /// Initializes a new instance of the <see cref="DwellingRowViewModel"/> class.
        /// </summary>
        /// <param name="built">The built.</param>
        /// <param name="waiting">The waiting.</param>
        /// <param name="isBuilt">The is built.</param>
        /// <param name="weeklyGrowth">The weekly growth.</param>
        public DwellingRowViewModel(ChoiceFieldViewModel built, AmountRowViewModel waiting, System.Func<bool> isBuilt, System.Func<int> weeklyGrowth)
        {
            this.weeklyGrowth = weeklyGrowth;
            this.Built = built;
            this.Waiting = waiting;
            this.isBuilt = isBuilt;
            this.AddWeekCommand = DwellingStockActions.AddWeekCommandOf(this);
        }

        /// <summary>
        /// Gets the field that sets how far the dwelling is built.
        /// </summary>
        public ChoiceFieldViewModel Built { get; }

        /// <summary>
        /// Gets the creatures that wait in the dwelling.
        /// </summary>
        public AmountRowViewModel Waiting { get; }

        /// <summary>
        /// Gets a value indicating whether the dwelling is built (as it is, or upgraded), so creatures can wait in it.
        /// </summary>
        public bool CanStock => this.isBuilt();

        /// <summary>
        /// Gets or sets how many creatures wait in the dwelling. Setting more than the dwelling can hold, or less than none, stops at the limit.
        /// </summary>
        public long Stock
        {
            get => this.Waiting.Value;
            set => this.Waiting.Value = value;
        }

        /// <summary>
        /// Gets how many creatures the dwelling gains each week, 0 when it is not built.
        /// </summary>
        public int WeeklyGrowthAmount => this.weeklyGrowth();

        /// <summary>
        /// Gets the command that adds a week of growth to the dwelling.
        /// </summary>
        public ICommand AddWeekCommand { get; }

        /// <summary>
        /// Gets what the dwelling gains each week, with the Well and the second well: such as "+20 a week", nothing for a dwelling that is not built.
        /// </summary>
        public string WeeklyGrowthText => Heroes.Wpf.ViewModels.WeeklyGrowthText.Of(this.WeeklyGrowthAmount);

        /// <summary>
        /// Reads the model again and tells the view that every value may have changed.
        /// </summary>
        public void Refresh()
        {
            this.Built.Refresh();
            this.Waiting.Refresh();
            this.RaiseAllPropertiesChanged();
        }
    }
}
