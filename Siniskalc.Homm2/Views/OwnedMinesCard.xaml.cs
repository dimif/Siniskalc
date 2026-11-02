namespace Siniskalc.Homm2.Views
{
    using System.ComponentModel;
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

        private IOwnedMines? page;
        private int boundMine = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwnedMinesCard"/> class.
        /// </summary>
        public OwnedMinesCard()
        {
            this.InitializeComponent();
            AdvancedModeVisibility.Apply(this.hexCard);
            this.hexCard.IsVisibleChanged += (_, _) => this.ShowBytes();
            this.DataContextChanged += this.OnDataContextChanged;
        }

        /// <summary>
        /// Gets or sets the line above the list of mines.
        /// </summary>
        public string Note
        {
            get => (string)this.GetValue(NoteProperty);
            set => this.SetValue(NoteProperty, value);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.page != null)
            {
                this.page.Mines.PropertyChanged -= this.OnMinesChanged;
            }

            this.page = e.NewValue as IOwnedMines;
            this.boundMine = -1;
            if (this.page != null)
            {
                this.page.Mines.PropertyChanged += this.OnMinesChanged;
            }

            this.ShowBytes();
        }

        private void OnMinesChanged(object? sender, PropertyChangedEventArgs e) => this.ShowBytes();

        // The bytes follow the mine that is picked; an edit of the same mine leaves the editor as it is. Building the grid of bytes is slow,
        // so it is only built while the card is on screen (advanced mode, on this tab).
        private void ShowBytes()
        {
            if (!this.hexCard.IsVisible)
            {
                this.boundMine = -1;
                return;
            }

            var picked = this.page?.Mines.SelectedMine?.Index ?? -1;
            if (picked == this.boundMine)
            {
                return;
            }

            this.boundMine = picked;
            if (this.page?.CreateMineByteEditTarget() is { } target)
            {
                this.hexEditor.Bind(target);
            }
            else
            {
                this.hexEditor.Unbind();
            }
        }
    }
}
