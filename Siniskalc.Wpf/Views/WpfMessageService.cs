namespace Siniskalc.Wpf.Views
{
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// Shows messages in message boxes owned by the window the user is working in, so a dialog never hides behind it.
    /// </summary>
    public sealed class WpfMessageService : IMessageService
    {
        public void ShowError(string title, string message) => Show(title, message, MessageBoxButton.OK, MessageBoxImage.Error);

        public void ShowUnexpectedError(string title, string message, string details)
        {
            var window = new ErrorWindow(title, message, details);
            var owner = OwnerWindow();
            if (owner is { IsLoaded: true })
            {
                window.Owner = owner;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            window.ShowDialog();
        }

        public void ShowWarning(string title, string message) => Show(title, message, MessageBoxButton.OK, MessageBoxImage.Warning);

        public void ShowInformation(string title, string message) => Show(title, message, MessageBoxButton.OK, MessageBoxImage.Information);

        public MessageAnswer AskYesNoCancel(string title, string message) => Show(title, message, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) switch
        {
            MessageBoxResult.Yes => MessageAnswer.Yes,
            MessageBoxResult.No => MessageAnswer.No,
            _ => MessageAnswer.Cancel,
        };

        private static Window? OwnerWindow()
        {
            var application = Application.Current;
            return application?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? application?.MainWindow;
        }

        private static MessageBoxResult Show(string title, string message, MessageBoxButton buttons, MessageBoxImage image)
        {
            var owner = OwnerWindow();
            return owner is { IsLoaded: true }
                ? MessageBox.Show(owner, message, title, buttons, image)
                : MessageBox.Show(message, title, buttons, image);
        }
    }
}
