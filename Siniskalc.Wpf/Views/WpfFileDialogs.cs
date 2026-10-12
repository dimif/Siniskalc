namespace Siniskalc.Wpf.Views
{
    using Microsoft.Win32;

    /// <summary>
    /// The file dialogs of Windows.
    /// </summary>
    public sealed class WpfFileDialogs : IFileDialogs
    {
        public string? PickFileToOpen(string filter)
        {
            var dialog = new OpenFileDialog { Filter = filter };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? PickFileToSave(string filter, string? fileName = null, string? defaultExtension = null)
        {
            var dialog = new SaveFileDialog { Filter = filter };
            if (fileName != null)
            {
                dialog.FileName = fileName;
            }

            if (defaultExtension != null)
            {
                dialog.DefaultExt = defaultExtension;
                dialog.AddExtension = true;
            }

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
