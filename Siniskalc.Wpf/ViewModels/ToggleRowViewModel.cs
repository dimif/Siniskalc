namespace Siniskalc.Wpf.ViewModels
{
    using System;

    /// <summary>
    /// A yes or no the user edits with a check box: a building of a town that is built or not. It reads its value from the model and
    /// writes every edit back through a delegate. It knows no game. Call <see cref="Refresh"/> after the model changed.
    /// </summary>
    public sealed class ToggleRowViewModel : AbstractViewModelBase, IToggleRow
    {
        private readonly Func<bool> get;
        private readonly Action<bool> set;
        private readonly Func<bool>? canToggle;
        private readonly Func<string>? note;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleRowViewModel"/> class.
        /// </summary>
        /// <param name="name">What it is, such as "Tavern".</param>
        /// <param name="get">Gives the value in the model as it is now.</param>
        /// <param name="set">Writes a value to the model.</param>
        /// <param name="canToggle">Tells whether the box can be changed right now; always when omitted.</param>
        /// <param name="note">Gives a small text under the name, such as a warning; none when omitted.</param>
        /// <param name="toolTip">What the box does.</param>
        public ToggleRowViewModel(string name, Func<bool> get, Action<bool> set, Func<bool>? canToggle = null, Func<string>? note = null, string toolTip = "")
        {
            this.Name = name;
            this.get = get;
            this.set = set;
            this.canToggle = canToggle;
            this.note = note;
            this.ToolTip = toolTip;
        }

        /// <summary>
        /// Gets the name of what the row switches.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the tooltip of the row.
        /// </summary>
        public string ToolTip { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the row is switched on.
        /// </summary>
        public bool IsChecked
        {
            get => this.get();
            set
            {
                if (value != this.get())
                {
                    this.set(value);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user can switch the row now.
        /// </summary>
        public bool CanToggle => this.canToggle?.Invoke() ?? true;

        /// <summary>
        /// Gets the small text under the name, or nothing.
        /// </summary>
        public string Note => this.note?.Invoke() ?? string.Empty;

        /// <summary>
        /// Shows the row again. Call after its value changed elsewhere.
        /// </summary>
        public void Refresh() => this.RaiseAllPropertiesChanged();
    }
}
