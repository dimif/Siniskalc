namespace Siniskalc.Wpf.ViewModels
{
    using System;

    /// <summary>
    /// One text the user types: an answer of a sphinx, its riddle. It reads its value from the model and writes every edit back through a
    /// delegate; the text cannot be longer than <see cref="MaxLength"/>. It knows no game. Call <see cref="Refresh"/> after the model changed.
    /// </summary>
    public sealed class TextFieldViewModel : AbstractViewModelBase
    {
        private readonly Func<string> get;
        private readonly Action<string> set;
        private readonly Func<int>? maxLength;

        /// <param name="label">What it is, such as "Answer 1".</param>
        /// <param name="get">Gives the text in the model as it is now.</param>
        /// <param name="set">Writes a typed text to the model.</param>
        /// <param name="maxLength">Gives how many characters it can have; no limit when omitted.</param>
        /// <param name="isMultiline">Whether the text has several lines and a taller box.</param>
        /// <param name="toolTip">What the field is for.</param>
        public TextFieldViewModel(string label, Func<string> get, Action<string> set, Func<int>? maxLength = null, bool isMultiline = false, string toolTip = "")
        {
            this.Label = label;
            this.get = get;
            this.set = set;
            this.maxLength = maxLength;
            this.IsMultiline = isMultiline;
            this.ToolTip = toolTip;
        }

        public string Label { get; }

        public string ToolTip { get; }

        public bool IsMultiline { get; }

        /// <summary>
        /// Gets how many characters the text can have, or 0 for no limit.
        /// </summary>
        public int MaxLength => this.maxLength?.Invoke() ?? 0;

        /// <summary>
        /// Gets how much of the room the text uses, such as "37 of 198", or nothing when there is no limit.
        /// </summary>
        public string Counter => this.MaxLength > 0 ? $"{this.Text.Length} of {this.MaxLength}" : string.Empty;

        public string Text
        {
            get => this.get();
            set
            {
                if ((value ?? string.Empty) != this.get())
                {
                    this.set(value ?? string.Empty);
                }
            }
        }

        public void Refresh() => this.RaiseAllPropertiesChanged();
    }
}
