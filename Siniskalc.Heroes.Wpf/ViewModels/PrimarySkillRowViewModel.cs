namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Globalization;
    using Siniskalc.Core;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One primary skill of a hero, in any game: the base the hero learned (edited), the total with what the worn artifacts add (what the
    /// savegame holds and what counts in the game) and a note of what the artifacts add. Call <see cref="Refresh"/> after the hero changed.
    /// </summary>
    public sealed class PrimarySkillRowViewModel : AbstractViewModelBase
    {
        private readonly Func<int> getBase;
        private readonly Action<int> setBase;
        private readonly Func<int> getTotal;
        private readonly Func<int> getBonus;

        /// <param name="label">The name of the skill, such as "Attack".</param>
        /// <param name="range">The limits of the base value.</param>
        /// <param name="getBase">Reads the base value of the skill.</param>
        /// <param name="setBase">Writes a new base value (within the limits and different from the one there is); the artifacts stay on top of it.</param>
        /// <param name="getTotal">Reads the skill with what the artifacts add.</param>
        /// <param name="getBonus">Reads what the artifacts add.</param>
        public PrimarySkillRowViewModel(string label, NumberRange range, Func<int> getBase, Action<int> setBase, Func<int> getTotal, Func<int> getBonus)
        {
            this.Label = label;
            this.Range = range;
            this.getBase = getBase;
            this.setBase = setBase;
            this.getTotal = getTotal;
            this.getBonus = getBonus;
        }

        /// <summary>
        /// Gets the name of the skill.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Gets the limits of the base value, for the stepper.
        /// </summary>
        public NumberRange Range { get; }

        /// <summary>
        /// Gets or sets the base value of the skill, what the hero has without its artifacts. A value out of the limits is brought within them.
        /// </summary>
        public long Base
        {
            get => RangeGuard.Clamp<long>(this.getBase(), this.Range.Minimum, this.Range.Maximum);
            set
            {
                var clamped = RangeGuard.Clamp(value, this.Range.Minimum, this.Range.Maximum);
                if (clamped != this.Base)
                {
                    this.setBase((int)clamped);
                }
            }
        }

        /// <summary>
        /// Gets the skill with what the artifacts add, which is what the savegame holds and what counts in the game.
        /// </summary>
        public int Total => this.getTotal();

        /// <summary>
        /// Gets what the artifacts add, such as "+2 from artifacts", or nothing when they add nothing.
        /// </summary>
        public string ArtifactNote => this.getBonus() is var bonus && bonus != 0
            ? bonus.ToString("+0;-0", CultureInfo.InvariantCulture) + " from artifacts"
            : string.Empty;

        /// <summary>
        /// Shows the hero again. Call after the hero changed.
        /// </summary>
        public void Refresh() => this.RaiseAllPropertiesChanged();
    }
}
