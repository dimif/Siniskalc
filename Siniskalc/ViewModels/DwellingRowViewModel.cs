namespace Siniskalc.ViewModels
{
    using System;
    using Siniskalc.Core;

    /// <summary>
    /// A creature level of the castle, as a line of the page: the dwelling that serves it and the creatures waiting in it.
    /// </summary>
    public sealed class DwellingRowViewModel
    {
        private readonly Action<int>? setCount;
        private long count;

        /// <param name="count">How many creatures wait in the dwelling, or null when no dwelling is built.</param>
        /// <param name="setCount">Called with the new number of creatures when the user edits it; null for a row that cannot be edited.</param>
        public DwellingRowViewModel(int level, string dwelling, string state, string creatures, bool isBuilt, int? count = null, Action<int>? setCount = null, string creatureNames = "")
        {
            this.CreatureNames = creatureNames;
            this.setCount = setCount;
            this.count = count ?? 0;
            this.CanEdit = isBuilt && setCount != null;
            this.Level = level;
            this.Dwelling = dwelling;
            this.State = state;
            this.Creatures = creatures;
            this.IsBuilt = isBuilt;
        }

        public int Level { get; }

        /// <summary>
        /// Gets the most creatures a dwelling can hold here.
        /// </summary>
        public long Maximum => Constants.Castle.MaxDwellingCreatures;

        public bool CanEdit { get; }

        /// <summary>
        /// Gets or sets how many creatures wait in the dwelling.
        /// </summary>
        public long Count
        {
            get => this.count;
            set
            {
                this.count = RangeGuard.Clamp(value, Constants.Castle.MinDwellingCreatures, this.Maximum);
                this.setCount?.Invoke((int)this.count);
            }
        }

        public string Dwelling { get; }

        /// <summary>
        /// Gets the creatures of the level, such as "Behemoth\Ancient Behemoth" when the upgraded dwelling is built, otherwise "Behemoth".
        /// </summary>
        public string CreatureNames { get; }

        /// <summary>Gets "Upgraded", "Built", "Not built" or "Not allowed by the map".</summary>
        public string State { get; }

        public string Creatures { get; }

        public bool IsBuilt { get; }
    }
}
