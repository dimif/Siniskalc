namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A button of the Banishing card: one number of days a player with no town can be given.
    /// </summary>
    public sealed class DaysLeftChoiceViewModel : AbstractViewModelBase
    {
        private readonly Func<bool> isCurrent;

        public DaysLeftChoiceViewModel(int days, Func<bool> isCurrent, Action select, Func<bool> canSelect)
        {
            this.Days = days;
            this.isCurrent = isCurrent;
            this.SelectCommand = new DelegateCommand(_ => select(), _ => canSelect());
        }

        public int Days { get; }

        /// <summary>
        /// Gets whether the player has this many days left now.
        /// </summary>
        public bool IsCurrent => this.isCurrent();

        public ICommand SelectCommand { get; }

        public void Refresh() => this.RaisePropertyChanged(nameof(this.IsCurrent));
    }
}
