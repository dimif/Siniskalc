namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One of the six dwellings of a castle: how far it is built (not built, built, upgraded) and how many creatures wait in it.
    /// </summary>
    public sealed class DwellingRowViewModel : AbstractViewModelBase
    {
        private readonly System.Func<bool> isBuilt;
        private readonly System.Func<int> weeklyGrowth;

        public DwellingRowViewModel(ChoiceFieldViewModel built, AmountRowViewModel waiting, System.Func<bool> isBuilt, System.Func<int> weeklyGrowth)
        {
            this.weeklyGrowth = weeklyGrowth;
            this.Built = built;
            this.Waiting = waiting;
            this.isBuilt = isBuilt;
        }

        public ChoiceFieldViewModel Built { get; }

        public AmountRowViewModel Waiting { get; }

        /// <summary>
        /// Gets a value indicating whether the dwelling is built (as it is, or upgraded).
        /// </summary>
        public bool IsBuilt => this.isBuilt();

        /// <summary>
        /// Gets what the dwelling gains each week, with the Well and the second well: such as "+20 a week", nothing for a dwelling that is not built.
        /// </summary>
        public string WeeklyGrowthText => this.weeklyGrowth() is var growth && growth > 0 ? $"+{growth} a week" : string.Empty;

        public void Refresh()
        {
            this.Built.Refresh();
            this.Waiting.Refresh();
            this.RaiseAllPropertiesChanged();
        }
    }
}
