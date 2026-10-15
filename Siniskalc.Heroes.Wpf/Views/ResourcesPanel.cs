namespace Siniskalc.Heroes.Wpf.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// The resources of a player: a card for each, the quick add buttons, what a game adds in between (its content) and a note about
    /// the limits. The data context has <c>Resources</c> (amount rows), <c>SetAllResourcesCommand</c> and <c>SetRichCommand</c>.
    /// </summary>
    public class ResourcesPanel : ContentControl
    {
        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(string), typeof(ResourcesPanel), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the note under the resources, which tells their limits.
        /// </summary>
        public string Note
        {
            get => (string)this.GetValue(NoteProperty);
            set => this.SetValue(NoteProperty, value);
        }
    }
}
