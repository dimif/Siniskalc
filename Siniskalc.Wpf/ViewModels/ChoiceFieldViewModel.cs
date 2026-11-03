namespace Siniskalc.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// One value picked from a list of names with a combo box: the owner of a town, the level of a guild, a spell of a guild. It reads
    /// its value from the model and writes every pick back through a delegate. It knows no game. Call <see cref="Refresh"/> after the
    /// model changed.
    /// </summary>
    public sealed class ChoiceFieldViewModel : AbstractViewModelBase
    {
        private readonly Func<string> get;
        private readonly Action<string> set;
        private readonly Func<bool>? isEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceFieldViewModel"/> class.
        /// </summary>
        /// <param name="label">What it is, such as "Owner".</param>
        /// <param name="choices">The names to pick from.</param>
        /// <param name="get">Gives the name of the value in the model as it is now.</param>
        /// <param name="set">Writes a picked name to the model.</param>
        /// <param name="isEnabled">Tells whether it can be changed right now; always when omitted.</param>
        /// <param name="toolTip">What the field is for.</param>
        public ChoiceFieldViewModel(string label, IReadOnlyList<string> choices, Func<string> get, Action<string> set, Func<bool>? isEnabled = null, string toolTip = "")
        {
            this.Label = label;
            this.Choices = choices;
            this.get = get;
            this.set = set;
            this.isEnabled = isEnabled;
            this.ToolTip = toolTip;
        }

        /// <summary>
        /// Gets the label shown next to the field.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Gets the values the user can pick from.
        /// </summary>
        public IReadOnlyList<string> Choices { get; }

        /// <summary>
        /// Gets the tooltip of the field.
        /// </summary>
        public string ToolTip { get; }

        /// <summary>
        /// Gets a value indicating whether the user can change the field now.
        /// </summary>
        public bool IsEnabled => this.isEnabled?.Invoke() ?? true;

        /// <summary>
        /// Gets or sets the choice that is selected.
        /// </summary>
        public string Selected
        {
            get => this.get();
            set
            {
                if (!string.IsNullOrEmpty(value) && value != this.get())
                {
                    this.set(value);
                }
            }
        }

        /// <summary>
        /// Shows the field again. Call after the value changed elsewhere.
        /// </summary>
        public void Refresh() => this.RaiseAllPropertiesChanged();
    }
}
