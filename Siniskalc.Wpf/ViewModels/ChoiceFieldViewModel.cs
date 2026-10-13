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

        public string Label { get; }

        public IReadOnlyList<string> Choices { get; }

        public string ToolTip { get; }

        public bool IsEnabled => this.isEnabled?.Invoke() ?? true;

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

        public void Refresh() => this.RaiseAllPropertiesChanged();
    }
}
