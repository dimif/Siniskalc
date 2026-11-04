namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Windows.Input;
    using Siniskalc.Core;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// A creature level of the castle, as a line of the page: the dwelling that serves it and the creatures waiting in it.
    /// </summary>
    public sealed class DwellingRowViewModel : IStockedDwelling
    {
        private readonly Action<int>? setCount;
        private long stock;

        /// <param name="count">How many creatures wait in the dwelling, or null when no dwelling is built.</param>
        /// <param name="setCount">Called with the new number of creatures when the user edits it; null for a row that cannot be edited.</param>
        /// <param name="weeklyGrowth">The base weekly growth of the creatures of the level (<see cref="Models.CastleModel.BaseWeeklyGrowth"/>), 0 when no dwelling is built.</param>
        public DwellingRowViewModel(int level, string dwelling, string state, string creatures, bool isBuilt, int? count = null, Action<int>? setCount = null, string creatureNames = "", int weeklyGrowth = 0)
        {
            this.WeeklyGrowthText = Heroes.Wpf.ViewModels.WeeklyGrowthText.Of(weeklyGrowth);
            this.WeeklyGrowthAmount = weeklyGrowth;
            this.AddWeekCommand = DwellingStockActions.AddWeekCommandOf(this);
            this.CreatureNames = creatureNames;
            this.setCount = setCount;
            this.stock = count ?? 0;
            this.CanStock = isBuilt && setCount != null;
            this.Level = level;
            this.Dwelling = dwelling;
            this.State = state;
            this.Creatures = creatures;
            this.IsBuilt = isBuilt;
        }

        /// <summary>
        /// Gets the level of the creatures the dwelling recruits.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Gets the most creatures a dwelling can hold here.
        /// </summary>
        public long Maximum => Constants.Castle.MaxDwellingCreatures;

        /// <summary>
        /// Gets a value indicating whether the creatures that wait can be changed: the dwelling is built and there is an action to write them.
        /// </summary>
        public bool CanStock { get; }

        /// <summary>
        /// Gets or sets how many creatures wait in the dwelling. Setting more than the dwelling can hold, or less than none, stops at the limit.
        /// </summary>
        public long Stock
        {
            get => this.stock;
            set
            {
                this.stock = RangeGuard.Clamp(value, Constants.Castle.MinDwellingCreatures, this.Maximum);
                this.setCount?.Invoke((int)this.stock);
            }
        }

        /// <summary>
        /// Gets the name of the dwelling.
        /// </summary>
        public string Dwelling { get; }

        /// <summary>
        /// Gets the creatures of the level, such as "Behemoth\Ancient Behemoth" when the upgraded dwelling is built, otherwise "Behemoth".
        /// </summary>
        public string CreatureNames { get; }

        /// <summary>Gets "Upgraded", "Built", "Not built" or "Not allowed by the map".</summary>
        public string State { get; }

        /// <summary>
        /// Gets the name of the creatures it recruits.
        /// </summary>
        public string Creatures { get; }

        /// <summary>
        /// Gets whether the dwelling is built.
        /// </summary>
        public bool IsBuilt { get; }

        /// <summary>
        /// Gets the base weekly growth of the creatures, such as "+14 a week", or nothing for a dwelling that is not built.
        /// </summary>
        public string WeeklyGrowthText { get; }

        /// <summary>
        /// Gets the base weekly growth of the creatures, 0 for a dwelling that is not built.
        /// </summary>
        public int WeeklyGrowthAmount { get; }

        /// <summary>
        /// Gets the command that adds a week of growth to the dwelling.
        /// </summary>
        public ICommand AddWeekCommand { get; }
    }
}
