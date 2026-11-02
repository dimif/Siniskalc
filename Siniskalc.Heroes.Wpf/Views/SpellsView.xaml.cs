namespace Siniskalc.Heroes.Wpf.Views
{
    using System.Windows.Controls;

    /// <summary>
    /// The spells of a hero: the spell book, and the list of every spell with a box for each learned one, in every game. All behaviour is
    /// in a <c>SpellsViewModelBase</c>; the control only binds to it.
    /// </summary>
    public partial class SpellsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpellsView"/> class.
        /// </summary>
        public SpellsView()
        {
            this.InitializeComponent();
        }
    }
}
