namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;

    /// <summary>
    /// Shows the mines of one owner with the guard of the mine that is picked, and the bytes of that mine: the shared owned-mines card with the
    /// guard editor of HoMM3 in it. All behaviour is in <see cref="OwnedMapObjectsViewModel"/>; the control only binds to it.
    /// </summary>
    public partial class OwnedMinesCards : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OwnedMinesCards"/> class.
        /// </summary>
        public OwnedMinesCards()
        {
            this.InitializeComponent();
        }
    }
}
