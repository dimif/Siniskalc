namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Globalization;
    using Siniskalc.Core;

    /// <summary>
    /// One number the user edits with a stepper: a resource of a player, a skill or the experience of a hero. It reads its value
    /// from the model, writes every edit back through a delegate, and knows how far the value is from what the savegame had.
    /// It knows no game: the game's view model gives it a name, the limits and the delegates. Call <see cref="Refresh"/> after the model changed.
    /// </summary>
    public class AmountRowViewModel : AbstractViewModelBase, IAmountRow
    {
        private readonly Func<long> current;
        private readonly Func<long> original;
        private readonly Action<long> set;
        private readonly Func<string>? note;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmountRowViewModel"/> class.
        /// </summary>
        /// <param name="name">What the number is, such as "Gold" or "Attack".</param>
        /// <param name="colorHex">The colour of the badge or marker of the row, e.g. "#B8902A".</param>
        /// <param name="minimum">The least the number can be given.</param>
        /// <param name="maximum">The most the number can be given.</param>
        /// <param name="step">How much one press of the buttons adds or removes.</param>
        /// <param name="current">Gives the value in the model as it is now.</param>
        /// <param name="original">Gives the value the savegame had when it was loaded.</param>
        /// <param name="set">Writes a value (already within the limits) to the model.</param>
        /// <param name="note">Gives a small text under the number, such as the income of a resource; none when null.</param>
        public AmountRowViewModel(string name, string colorHex, long minimum, long maximum, long step, Func<long> current, Func<long> original, Action<long> set, Func<string>? note = null)
        {
            this.Name = name;
            this.ColorHex = colorHex;
            this.Minimum = minimum;
            this.Maximum = maximum;
            this.Step = step;
            this.current = current;
            this.original = original;
            this.set = set;
            this.note = note;
        }

        /// <summary>
        /// Gets the name of what the amount counts, such as a resource.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the first two letters of the name, shown on the badge of the row.
        /// </summary>
        public string Badge => this.Name.Length < 2 ? this.Name : this.Name[..2];

        /// <summary>
        /// Gets the colour of the row as a hex string such as <c>#FFD700</c>.
        /// </summary>
        public string ColorHex { get; }

        /// <summary>
        /// Gets the least amount the row accepts.
        /// </summary>
        public long Minimum { get; }

        /// <summary>
        /// Gets the greatest amount the row accepts.
        /// </summary>
        public long Maximum { get; }

        /// <summary>
        /// Gets how much one press of the buttons adds or removes.
        /// </summary>
        public long Step { get; }

        /// <summary>
        /// Gets or sets the amount, kept between <see cref="Minimum"/> and <see cref="Maximum"/>.
        /// </summary>
        public long Value
        {
            get => this.current();
            set => this.set(RangeGuard.Clamp(value, this.Minimum, this.Maximum));
        }

        /// <summary>
        /// Gets a value indicating whether the amount differs from the loaded one.
        /// </summary>
        public bool IsChanged => this.Value != this.original();

        /// <summary>
        /// Gets how far the value is from the savegame's, such as "+17 from save", or nothing when it is the same.
        /// </summary>
        public string DeltaText
        {
            get
            {
                var delta = this.Value - this.original();
                return delta == 0 ? string.Empty : $"{(delta > 0 ? "+" : string.Empty)}{delta.ToString("N0", CultureInfo.CurrentCulture)} from save";
            }
        }

        /// <summary>
        /// Gets the small text under the number, or nothing.
        /// </summary>
        public string Note => this.note?.Invoke() ?? string.Empty;

        /// <summary>
        /// Shows the amount again. Call after it changed elsewhere.
        /// </summary>
        public void Refresh() => this.RaiseAllPropertiesChanged();
    }
}
