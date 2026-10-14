namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One of the six dwellings of a castle: how far it is built (not built, built, upgraded) and how many creatures wait in it.
    /// </summary>
    public sealed class DwellingRowViewModel : AbstractViewModelBase
    {
        private readonly System.Func<bool> isBuilt;

        public DwellingRowViewModel(ChoiceFieldViewModel built, AmountRowViewModel waiting, System.Func<bool> isBuilt)
        {
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

        public void Refresh()
        {
            this.Built.Refresh();
            this.Waiting.Refresh();
            this.RaiseAllPropertiesChanged();
        }
    }
}
