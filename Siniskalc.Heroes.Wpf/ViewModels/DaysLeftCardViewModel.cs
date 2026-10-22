namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The Banishing card of a player page, in any game: the days a player who owns no town has left before it is out of the game, as
    /// one button for each number of days, <see cref="Constants.Player.MaxDaysLeft"/> down to 1. The card is shown only while the player
    /// owns no town, and a banished player is shown but cannot be given days.
    /// </summary>
    public sealed class DaysLeftCardViewModel : AbstractViewModelBase
    {
        private readonly Func<int?> getDaysLeft;
        private readonly Action<int> setDaysLeft;
        private readonly Func<bool> isBanished;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaysLeftCardViewModel"/> class.
        /// </summary>
        /// <param name="getDaysLeft">Reads the days left of the player, <c>null</c> while it owns a town.</param>
        /// <param name="setDaysLeft">Gives the player the days, 1 to <see cref="Constants.Player.MaxDaysLeft"/>.</param>
        /// <param name="isBanished">Tells whether the countdown of the player has run out.</param>
        public DaysLeftCardViewModel(Func<int?> getDaysLeft, Action<int> setDaysLeft, Func<bool> isBanished)
        {
            this.getDaysLeft = getDaysLeft;
            this.setDaysLeft = setDaysLeft;
            this.isBanished = isBanished;
            this.DayChoices = Enumerable.Range(Constants.Player.MinDaysLeft + 1, Constants.Player.MaxDaysLeft - Constants.Player.MinDaysLeft)
                .Reverse()
                .Select(days => new DaysLeftChoiceViewModel(days, () => this.DaysLeft == days, () => this.SetDaysLeft(days), () => !this.IsBanished))
                .ToList();
        }

        /// <summary>
        /// Gets the days left before the player is out of the game, or <c>null</c> while the player owns a town.
        /// </summary>
        public int? DaysLeft => this.getDaysLeft();

        /// <summary>
        /// Gets whether the card is shown: the player owns no town.
        /// </summary>
        public bool HasBanishing => this.DaysLeft != null;

        /// <summary>
        /// Gets whether the countdown of the player has run out, so that it cannot be given days.
        /// </summary>
        public bool IsBanished => this.isBanished();

        /// <summary>
        /// Gets the days the player can be given, <see cref="Constants.Player.MaxDaysLeft"/> down to 1.
        /// </summary>
        public IReadOnlyList<DaysLeftChoiceViewModel> DayChoices { get; }

        /// <summary>
        /// Gives the player <paramref name="days"/> left. A banished player is left alone.
        /// </summary>
        /// <param name="days">The days, 1 to <see cref="Constants.Player.MaxDaysLeft"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="days"/> is out of that range.</exception>
        public void SetDaysLeft(int days)
        {
            RangeGuard.ThrowIfOutOfRange(days, Constants.Player.MinDaysLeft + 1, Constants.Player.MaxDaysLeft, nameof(days));
            if (!this.IsBanished)
            {
                this.setDaysLeft(days);
            }
        }

        /// <summary>
        /// Shows the player again. Call after the player changed.
        /// </summary>
        public void Refresh()
        {
            this.RaiseAllPropertiesChanged();
            foreach (var choice in this.DayChoices)
            {
                choice.Refresh();
            }
        }
    }
}
