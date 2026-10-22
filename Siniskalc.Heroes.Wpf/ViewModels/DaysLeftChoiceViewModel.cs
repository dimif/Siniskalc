namespace Siniskalc.Heroes.Wpf.ViewModels
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DaysLeftChoiceViewModel"/> class.
        /// </summary>
        /// <param name="days">The days the button gives.</param>
        /// <param name="isCurrent">Tells whether the player has this many days left now.</param>
        /// <param name="select">Gives the player the days.</param>
        /// <param name="canSelect">Tells whether the player can be given days.</param>
        public DaysLeftChoiceViewModel(int days, Func<bool> isCurrent, Action select, Func<bool> canSelect)
        {
            this.Days = days;
            this.isCurrent = isCurrent;
            this.SelectCommand = new DelegateCommand(_ => select(), _ => canSelect());
        }

        /// <summary>
        /// Gets the days the button gives.
        /// </summary>
        public int Days { get; }

        /// <summary>
        /// Gets whether the player has this many days left now.
        /// </summary>
        public bool IsCurrent => this.isCurrent();

        /// <summary>
        /// Gets the command that gives the player the days.
        /// </summary>
        public ICommand SelectCommand { get; }

        /// <summary>
        /// Shows whether the button is the current one again. Call after the player changed.
        /// </summary>
        public void Refresh() => this.RaisePropertyChanged(nameof(this.IsCurrent));
    }
}
