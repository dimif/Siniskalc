namespace Siniskalc.Wpf.Views
{
    using System.Windows.Controls;

    /// <summary>
    /// The slots of an army and the finder to add creatures, shared by the creatures of a hero and the garrison of a castle, in every game.
    /// All behaviour is in an <c>ArmyViewModelBase</c>; the control only binds to it.
    /// </summary>
    public partial class ArmyEditorView : UserControl
    {
        public ArmyEditorView()
        {
            this.InitializeComponent();
        }
    }
}
