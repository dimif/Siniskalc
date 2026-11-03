namespace Siniskalc.Homm2.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// Shows the mines of one owner and the bytes of the mine that is picked. All behaviour is in the page it is the data context of
    /// (<see cref="IOwnedMines"/>); the control only binds to it and shows the bytes of the mine that is picked.
    /// </summary>
    public partial class OwnedMinesCard : UserControl
    {
        /// <summary>
        /// The line above the list of mines.
        /// </summary>
        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(
            nameof(Note),
            typeof(string),
            typeof(OwnedMinesCard),
            new PropertyMetadata(string.Empty, (d, e) => ((OwnedMinesCard)d).minesCard.Note = (string)e.NewValue));

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
                page => page.Mines,
                page => page.Mines.SelectedMine?.Index,
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
    }
}
