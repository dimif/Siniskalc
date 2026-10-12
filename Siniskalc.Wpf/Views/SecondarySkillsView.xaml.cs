namespace Siniskalc.Wpf.Views
{
    using System.Windows.Controls;

    /// <summary>
    /// The slots of the secondary skills of a hero and the list of every skill, in every game. All behaviour is in a
    /// <c>SecondarySkillsViewModelBase</c>; the control only binds to it.
    /// </summary>
    public partial class SecondarySkillsView : UserControl
    {
        public SecondarySkillsView()
        {
            this.InitializeComponent();
        }
    }
}
