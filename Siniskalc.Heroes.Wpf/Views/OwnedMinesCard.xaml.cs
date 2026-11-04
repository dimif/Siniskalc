namespace Siniskalc.Heroes.Wpf.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// Shows the mines of one owner in any game, what the game shows of the mine that is picked (<see cref="Detail"/>, such as the editor of its
    /// guard) and the bytes of that mine. All behaviour is in the page it is the data context of (<see cref="IOwnedMines"/>); the control only
    /// binds to it and shows the bytes of the mine that is picked.
    /// </summary>
    public partial class OwnedMinesCard : UserControl
    {
        /// <summary>
        /// Identifies the <see cref="Note"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(
            nameof(Note),
            typeof(string),
            typeof(OwnedMinesCard),
            new PropertyMetadata(string.Empty, (d, e) => ((OwnedMinesCard)d).minesCard.Note = (string)e.NewValue));

        /// <summary>
        /// Identifies the <see cref="SummaryHeading"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SummaryHeadingProperty = DependencyProperty.Register(
            nameof(SummaryHeading),
            typeof(string),
            typeof(OwnedMinesCard),
            new PropertyMetadata(string.Empty, (d, e) => ((OwnedMinesCard)d).minesCard.SummaryHeading = (string)e.NewValue));

        /// <summary>
        /// Identifies the <see cref="ShowLevel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowLevelProperty = DependencyProperty.Register(
            nameof(ShowLevel),
            typeof(bool),
            typeof(OwnedMinesCard),
            new PropertyMetadata(false, (d, e) => ((OwnedMinesCard)d).minesCard.ShowLevel = (bool)e.NewValue));

        /// <summary>
        /// Identifies the <see cref="Detail"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DetailProperty = DependencyProperty.Register(
            nameof(Detail),
            typeof(object),
            typeof(OwnedMinesCard),
            new PropertyMetadata(null, (d, e) => ((OwnedMinesCard)d).detail.Content = e.NewValue));

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnedMinesCard"/> class.
        /// </summary>
        public OwnedMinesCard()
        {
            this.InitializeComponent();
            AdvancedModeVisibility.Apply(this.hexCard);
            _ = new SelectedRecordHexPresenter<IOwnedMines>(
                this,
                this.hexCard,
                this.hexEditor,
                page => page.MineSelection,
                page => page.SelectedMineIndex,
                page => page.CreateMineByteEditTarget());
        }

        /// <summary>
        /// Gets or sets the line above the list of mines.
        /// </summary>
        public string Note
        {
            get => (string)this.GetValue(NoteProperty);
            set => this.SetValue(NoteProperty, value);
        }

        /// <summary>
        /// Gets or sets the heading of the last column of the list, which the game fills with what it knows of a mine besides its owner.
        /// </summary>
        public string SummaryHeading
        {
            get => (string)this.GetValue(SummaryHeadingProperty);
            set => this.SetValue(SummaryHeadingProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the list has a column for the level of the map, for a game whose map has two.
        /// </summary>
        public bool ShowLevel
        {
            get => (bool)this.GetValue(ShowLevelProperty);
            set => this.SetValue(ShowLevelProperty, value);
        }

        /// <summary>
        /// Gets or sets what the card shows between the list and the bytes, such as the editor of the guard of the mine that is picked; it has
        /// the page as its data context. None by default.
        /// </summary>
        public object? Detail
        {
            get => this.GetValue(DetailProperty);
            set => this.SetValue(DetailProperty, value);
        }
    }
}
