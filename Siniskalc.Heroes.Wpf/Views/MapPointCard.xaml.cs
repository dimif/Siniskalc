namespace Siniskalc.Heroes.Wpf.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;

    /// <summary>
    /// The card of the position or the marker of a hero. The data context is a <c>MapPointCardViewModel</c>.
    /// </summary>
    public partial class MapPointCard : UserControl
    {
        public MapPointCard()
        {
            this.InitializeComponent();
        }

        // The list of castles opens ready to be searched: the search box has the focus.
        private void OnCastlePopupOpened(object sender, EventArgs e) =>
            this.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                this.castleSearch.Focus();
                Keyboard.Focus(this.castleSearch);
            }));
    }
}
