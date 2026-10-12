namespace Siniskalc.Wpf.Views
{
    using System;
    using System.Windows;
    using Siniskalc.Core.Data;

    /// <summary>
    /// Shows every byte of the savefile with its known regions, read only. For finding out where things are in the savefile.
    /// </summary>
    public partial class SaveHexWindow : Window
    {
        /// <param name="dumpSource">Gives the savefile as it is now; asked again when the view is refreshed.</param>
        public SaveHexWindow(Func<SaveHexDump> dumpSource)
        {
            this.InitializeComponent();
            this.viewer.Bind(dumpSource);
        }
    }
}
