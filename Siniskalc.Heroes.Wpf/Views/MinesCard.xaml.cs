namespace Siniskalc.Heroes.Wpf.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// The mines of the map, for any game: a row for each with the headings that sort them. The data context is the page that has the list.
    /// </summary>
    public partial class MinesCard : UserControl
    {
        public static readonly DependencyProperty NoteProperty =
            DependencyProperty.Register(nameof(Note), typeof(string), typeof(MinesCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SummaryHeadingProperty =
            DependencyProperty.Register(nameof(SummaryHeading), typeof(string), typeof(MinesCard), new PropertyMetadata(string.Empty));

        public MinesCard()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the note under the heading of the card.
        /// </summary>
        public string Note
        {
            get => (string)this.GetValue(NoteProperty);
            set => this.SetValue(NoteProperty, value);
        }

        /// <summary>
        /// Gets or sets the heading of the last column, which a game fills with what it knows of a mine besides its owner.
        /// </summary>
        public string SummaryHeading
        {
            get => (string)this.GetValue(SummaryHeadingProperty);
            set => this.SetValue(SummaryHeadingProperty, value);
        }
    }
}
